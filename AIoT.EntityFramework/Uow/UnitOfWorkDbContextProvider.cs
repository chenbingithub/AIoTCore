using System;
using System.Collections.Generic;
using AIoT.Core;
using AIoT.Core.Data;
using AIoT.Core.Uow;
using AIoT.EntityFramework.EfCore;
using AIoT.EntityFramework.EfCore.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace AIoT.EntityFramework.Uow
{

    public class UnitOfWorkDbContextProvider<TDbContext> : IDbContextProvider<TDbContext>
        where TDbContext : IEfCoreDbContext
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IConnectionStringResolver _connectionStringResolver;
        private readonly IDataFilter _dataFilter;
        private readonly Dictionary<(Type Type, bool IsRead), TDbContext>
            _cacheDbContext = new Dictionary<(Type Type, bool IsRead), TDbContext>();

        public UnitOfWorkDbContextProvider(
            IUnitOfWorkManager unitOfWorkManager,
            IConnectionStringResolver connectionStringResolver, IServiceProvider serviceProvider, IDataFilter dataFilter)
        {
            _unitOfWorkManager = unitOfWorkManager;
            _connectionStringResolver = connectionStringResolver;
            _serviceProvider = serviceProvider;
            _dataFilter = dataFilter;
        }

        public TDbContext GetDbContext()
        {
            var unitOfWork = _unitOfWorkManager.Current;
            if (unitOfWork == null)
            {
                var cackeKey = (typeof(TDbContext), _dataFilter.IsEnabled<IReadonly>());
                return _cacheDbContext.GetOrAdd(cackeKey, p => _serviceProvider.GetRequiredService<TDbContext>());
            }
            var connectionStringName = typeof(TDbContext).Name;
            var connectionString = _connectionStringResolver.Resolve();

            var dbContextKey = $"{typeof(TDbContext).FullName}_{connectionString}";

            var databaseApi = unitOfWork.GetOrAddDatabaseApi(
                dbContextKey,
                () => new EfCoreDatabaseApi<TDbContext>(
                    CreateDbContext(unitOfWork, connectionStringName, connectionString)
                ));

            return ((EfCoreDatabaseApi<TDbContext>)databaseApi).DbContext;
        }

        private TDbContext CreateDbContext(IUnitOfWork unitOfWork, string connectionStringName, string connectionString)
        {
            var creationContext = new DbContextCreationContext(connectionStringName, connectionString);
            using (DbContextCreationContext.Use(creationContext))
            {
                var dbContext = CreateDbContext(unitOfWork);

                if (dbContext is IAbpEfCoreDbContext abpEfCoreDbContext)
                {
                    abpEfCoreDbContext.Initialize(
                        new AbpEfCoreDbContextInitializationContext(
                            unitOfWork
                        )
                    );
                }

                return dbContext;
            }
        }

        private TDbContext CreateDbContext(IUnitOfWork unitOfWork)
        {
            return unitOfWork?.Options.IsTransactional==true
                ? CreateDbContextWithTransaction(unitOfWork)
                : _serviceProvider.GetRequiredService<TDbContext>();
        }

        public TDbContext CreateDbContextWithTransaction(IUnitOfWork unitOfWork) 
        {
            var transactionApiKey = $"EntityFrameworkCore_{DbContextCreationContext.Current.ConnectionString}";
            var activeTransaction = unitOfWork.FindTransactionApi(transactionApiKey) as EfCoreTransactionApi;

            if (activeTransaction == null)
            {
                var dbContext = unitOfWork.ServiceProvider.GetRequiredService<TDbContext>();

                var dbtransaction = unitOfWork.Options.IsolationLevel.HasValue
                    ? dbContext.Database.BeginTransaction(unitOfWork.Options.IsolationLevel.Value)
                    : dbContext.Database.BeginTransaction();

                unitOfWork.AddTransactionApi(
                    transactionApiKey,
                    new EfCoreTransactionApi(
                        dbtransaction,
                        dbContext
                    )
                );

                return dbContext;
            }
            else
            {
                DbContextCreationContext.Current.ExistingConnection = activeTransaction.DbContextTransaction.GetDbTransaction().Connection;

                var dbContext = unitOfWork.ServiceProvider.GetRequiredService<TDbContext>();

                if (dbContext.As<DbContext>().HasRelationalTransactionManager())
                {
                    dbContext.Database.UseTransaction(activeTransaction.DbContextTransaction.GetDbTransaction());
                }
                else
                {
                    dbContext.Database.BeginTransaction(); //TODO: Why not using the new created transaction?
                }

                activeTransaction.AttendedDbContexts.Add(dbContext);

                return dbContext;
            }
        }
    }
}
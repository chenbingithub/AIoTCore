using System;
using System.Collections.Generic;
using AIoT.Core.DataFilter;
using AIoT.Core.Enums;
using AIoT.Core.Uow;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace AIoT.Core.EntityFrameworkCore
{

    public class UnitOfWorkDbContextProvider : IDbContextProvider, ITransientDependency

    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IConnectionStringResolver _connectionStringResolver;
        private readonly IDataState _dataState;
        private readonly Dictionary<(Type Type, bool IsRead), DbContext>
            _cacheDbContext = new Dictionary<(Type Type, bool IsRead), DbContext>();
        public UnitOfWorkDbContextProvider(
            IUnitOfWorkManager unitOfWorkManager,
            IConnectionStringResolver connectionStringResolver, IDataState dataState, IServiceProvider serviceProvider)
        {
            _unitOfWorkManager = unitOfWorkManager;
            _connectionStringResolver = connectionStringResolver;
            _dataState = dataState;
            _serviceProvider = serviceProvider;
        }

        public TDbContext GetDbContext<TDbContext>() where TDbContext : AbpDbContext
        {
            var unitOfWork = _unitOfWorkManager.Current;
            if (unitOfWork == null)
            {
                var cackeKey = (typeof(TDbContext), _dataState.IsEnabled(DataStateKeys.IsReadonly));
                return (TDbContext)_cacheDbContext.GetOrAdd(cackeKey, p => CreateDbContext<TDbContext>(null));
                //throw new AbpException("A DbContext can only be created inside a unit of work!");
            }
           
            var connectionStringName = typeof(TDbContext).Name;
            var connectionString = _connectionStringResolver.Resolve<TDbContext>();
            var dbContextKey = $"{typeof(TDbContext).FullName}:{unitOfWork.Options.IsTransactional}:{connectionString}";
            var databaseProvider = _connectionStringResolver.GetDatabaseProvider();
            var databaseApi = unitOfWork.GetOrAddDatabaseApi(
                dbContextKey,
                () => new EfCoreDatabaseApi<TDbContext>(
                    CreateDbContext<TDbContext>(unitOfWork, connectionStringName, connectionString, databaseProvider)
                ));

            return ((EfCoreDatabaseApi<TDbContext>)databaseApi).DbContext;
        }

        public TDbContext GetDbContext<TDbContext>(EfCoreDatabaseProvider databaseProvider) where TDbContext : AbpDbContext
        {
            
            var connectionStringName = $"{typeof(TDbContext).Name}_{databaseProvider.ToString()}";
            string connectionString=String.Empty;
            var options = _serviceProvider.GetService<IOptions<CustomOptions>>().Value;
            if (databaseProvider == EfCoreDatabaseProvider.SqlServer)
            {
                connectionString = options.SqlServerConnectionString;
            }else if (databaseProvider == EfCoreDatabaseProvider.MySql)
            {
                connectionString = options.MySqlConnectionString;
            }
            else if (databaseProvider == EfCoreDatabaseProvider.Oracle)
            {
                connectionString = options.OracleConnectionString;
            }

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception($"CustomOptions中没有配置{databaseProvider.ToString()}数据库字符串");
            }
            return CreateDbContext<TDbContext>(null, connectionStringName, connectionString, databaseProvider.ToString());

        }

        private TDbContext CreateDbContext<TDbContext>(IUnitOfWork unitOfWork, string connectionStringName, string connectionString,string databaseProvider)
            where TDbContext : AbpDbContext
        {
            var creationContext = new DbContextCreationContext(connectionStringName, connectionString, databaseProvider);
            using (DbContextCreationContext.Use(creationContext))
            {
                var dbContext = CreateDbContext<TDbContext>(unitOfWork);

                //if (dbContext is TDbContext abpEfCoreDbContext)
                //{
                //    abpEfCoreDbContext.Initialize(
                //        new AbpEfCoreDbContextInitializationContext(
                //            unitOfWork
                //        )
                //    );
                //}

                return dbContext;
            }
        }

        private TDbContext CreateDbContext<TDbContext>(IUnitOfWork unitOfWork) where TDbContext : AbpDbContext
        {
            return unitOfWork?.Options.IsTransactional==true
                ? CreateDbContextWithTransaction<TDbContext>(unitOfWork)
                : _serviceProvider.GetRequiredService<TDbContext>();
        }

        public TDbContext CreateDbContextWithTransaction<TDbContext>(IUnitOfWork unitOfWork)
             where TDbContext : AbpDbContext
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

                if (dbContext.As<DbContext>().Database.GetService<IDbContextTransactionManager>() is IRelationalTransactionManager)
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
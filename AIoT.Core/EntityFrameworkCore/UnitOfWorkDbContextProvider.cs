using System;
using AIoT.Core.Uow;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;

namespace AIoT.Core.EntityFrameworkCore
{

    public class UnitOfWorkDbContextProvider : IDbContextProvider
       
    {
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IConnectionStringResolver _connectionStringResolver;

        public UnitOfWorkDbContextProvider(
            IUnitOfWorkManager unitOfWorkManager,
            IConnectionStringResolver connectionStringResolver)
        {
            _unitOfWorkManager = unitOfWorkManager;
            _connectionStringResolver = connectionStringResolver;
        }

        public TDbContext GetDbContext<TDbContext>() where TDbContext : AbpDbContext
        {
            var unitOfWork = _unitOfWorkManager.Current;
            if (unitOfWork == null)
            {
                throw new AbpException("A DbContext can only be created inside a unit of work!");
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
            return unitOfWork.Options.IsTransactional
                ? CreateDbContextWithTransaction<TDbContext>(unitOfWork)
                : unitOfWork.ServiceProvider.GetRequiredService<TDbContext>();
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
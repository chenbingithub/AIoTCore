using System;
using System.Collections.Generic;
using System.Linq;
using AIoT.Core;
using AIoT.EntityFramework.Uow;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;

namespace AIoT.EntityFramework.EntityFrameworkCore
{

    public class UnitOfWorkDbContextProvider : IDbContextProvider, ITransientDependency

    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IConnectionStringResolver _connectionStringResolver;
        private readonly IDataFilter _dataFilter;
        private readonly Dictionary<(Type Type, bool IsRead), DbContext>
            _cacheDbContext = new Dictionary<(Type Type, bool IsRead), DbContext>();
        public UnitOfWorkDbContextProvider(
            IUnitOfWorkManager unitOfWorkManager,
            IConnectionStringResolver connectionStringResolver, IServiceProvider serviceProvider, IDataFilter dataFilter)
        {
            _unitOfWorkManager = unitOfWorkManager;
            _connectionStringResolver = connectionStringResolver;
            _serviceProvider = serviceProvider;
            _dataFilter = dataFilter;
        }

        public TDbContext GetDbContext<TDbContext>() where TDbContext : DbContext
        {
            var unitOfWork = _unitOfWorkManager.Current;
            if (unitOfWork == null)
            {
                var cackeKey = (typeof(TDbContext), _dataFilter.IsEnabled<IReadonly>());
                return (TDbContext)_cacheDbContext.GetOrAdd(cackeKey, p => CreateDbContext<TDbContext>(null));
                //throw new AbpException("A DbContext can only be created inside a unit of work!");
            }
           
            var connectionStringName = typeof(TDbContext).Name;
            var connectionString = _connectionStringResolver.Resolve<TDbContext>();
            var dbContextKey = $"{typeof(TDbContext).FullName}:{unitOfWork.Options.IsTransactional}:{connectionString}";
            var databaseProvider = _connectionStringResolver.GetDatabaseProvider();
            var databaseApi = unitOfWork.GetOrAddDatabaseApi(
                dbContextKey,
                () => new EntityFramework.EntityFrameworkCore.EfCoreDatabaseApi<TDbContext>(
                    CreateDbContext<TDbContext>(unitOfWork, connectionStringName, connectionString, databaseProvider)
                ));

            return ((EntityFramework.EntityFrameworkCore.EfCoreDatabaseApi<TDbContext>)databaseApi).DbContext;
        }

        public TDbContext GetDbContext<TDbContext>(string databaseKey) where TDbContext : DbContext
        {
            
            var connectionStringName = $"{typeof(TDbContext).Name}_{databaseKey}";
            string connectionString=String.Empty;
            var options = _serviceProvider.GetService<IOptions<EntityFramework.EntityFrameworkCore.CustomOptions>>().Value;
            if (options.Options == null || options.Options.Count <= 0)
            {
                throw new Exception($"CustomOptions中没有配置{databaseKey}数据库字符串");
            }

            var conn = options.Options.FirstOrDefault(u => u.DatabaseKey.ToLower() == databaseKey.ToLower());
           

            if (conn==null)
            {
                throw new Exception($"CustomOptions中没有配置{databaseKey.ToString()}数据库字符串");
            }

            connectionString = conn.ConnectionString;
            return CreateDbContext<TDbContext>(null, connectionStringName, conn.ConnectionString, conn.DatabaseProvider);

        }

        private TDbContext CreateDbContext<TDbContext>(IUnitOfWork unitOfWork, string connectionStringName, string connectionString,string databaseProvider)
            where TDbContext : DbContext
        {
            var creationContext = new EntityFramework.EntityFrameworkCore.DbContextCreationContext(connectionStringName, connectionString, databaseProvider);
            using (EntityFramework.EntityFrameworkCore.DbContextCreationContext.Use(creationContext))
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

        private TDbContext CreateDbContext<TDbContext>(IUnitOfWork unitOfWork) where TDbContext : DbContext
        {
            return unitOfWork?.Options.IsTransactional==true
                ? CreateDbContextWithTransaction<TDbContext>(unitOfWork)
                : _serviceProvider.GetRequiredService<TDbContext>();
        }

        public TDbContext CreateDbContextWithTransaction<TDbContext>(IUnitOfWork unitOfWork)
             where TDbContext : DbContext
        {
            var transactionApiKey = $"EntityFrameworkCore_{EntityFramework.EntityFrameworkCore.DbContextCreationContext.Current.ConnectionString}";
            var activeTransaction = unitOfWork.FindTransactionApi(transactionApiKey) as EntityFramework.EntityFrameworkCore.EfCoreTransactionApi;

            if (activeTransaction == null)
            {
                var dbContext = unitOfWork.ServiceProvider.GetRequiredService<TDbContext>();

                var dbtransaction = unitOfWork.Options.IsolationLevel.HasValue
                    ? dbContext.Database.BeginTransaction(unitOfWork.Options.IsolationLevel.Value)
                    : dbContext.Database.BeginTransaction();

                unitOfWork.AddTransactionApi(
                    transactionApiKey,
                    new EntityFramework.EntityFrameworkCore.EfCoreTransactionApi(
                        dbtransaction,
                        dbContext
                    )
                );

                return dbContext;
            }
            else
            {
                EntityFramework.EntityFrameworkCore.DbContextCreationContext.Current.ExistingConnection = activeTransaction.DbContextTransaction.GetDbTransaction().Connection;

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
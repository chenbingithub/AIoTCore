using System;
using System.Data;
using AIoT.Core.DataFilter;
using AIoT.Core.Entities.Auditing;
using AIoT.Core.EntityFrameworkCore;
using AIoT.Core.Enums;
using AIoT.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;
using AIoT.Core.Uow;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using AIoT.Core.Repository;
using Volo.Abp.Autofac;
using Volo.Abp.Reflection;

namespace AIoT.Core
{
    [DependsOn(typeof(AbpAutofacModule))]
    public class AIoTCoreModule : AbpModule
    {
        public override void PreConfigureServices(ServiceConfigurationContext context)
        {
            var services = context.Services;

            // Uow 拦截器
            services.OnRegistred(UnitOfWorkInterceptorRegistrar.RegisterIfNeeded);

            // Repository
            services.OnExposing(p =>
            {
                var list = new HashSet<Type>();
                if (typeof(IRepository).IsAssignableFrom(p.ImplementationType))
                {
                    // 注册泛型仓储接口：
                    // IReposiroty<TEntity>, IRepository<TEntity, TKey>
                    list.AddIfNotContains(ReflectionHelper.GetImplementedGenericTypes(
                        p.ImplementationType, typeof(IRepository<>)));
                    list.AddIfNotContains(ReflectionHelper.GetImplementedGenericTypes(
                        p.ImplementationType, typeof(IRepository<,>)));
                    // IReadOnlyRepository<TEntity> IReadOnlyRepository<TEntity, TKey>
                    list.AddIfNotContains(ReflectionHelper.GetImplementedGenericTypes(
                        p.ImplementationType, typeof(IReadRepository<>)));
                    list.AddIfNotContains(ReflectionHelper.GetImplementedGenericTypes(
                        p.ImplementationType, typeof(IReadRepository<,>)));
                    // IWriteRepository<TEntity>, IWriteRepository<TEntity, TKey>
                    list.AddIfNotContains(ReflectionHelper.GetImplementedGenericTypes(
                        p.ImplementationType, typeof(IWriteRepository<>)));
                    list.AddIfNotContains(ReflectionHelper.GetImplementedGenericTypes(
                        p.ImplementationType, typeof(IWriteRepository<,>)));
                }

                // Mediator 注册泛型 Handler
                //list.AddIfNotContains(p.ImplementationType.GetClosedGenericTypes(typeof(IRequestHandler<,>)));
                //list.AddIfNotContains(p.ImplementationType.GetClosedGenericTypes(typeof(INotificationHandler<>)));

                p.ExposedTypes.AddRange(list);

            });
        }
    

    public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var services = context.Services;
            var config = services.GetConfiguration();
            Configure<DbConnectionOptions>(config);
            Configure<CustomOptions>(config.GetSection("CustomOptions"));

            Configure<AbpUnitOfWorkDefaultOptions>(options =>
            {
                options.IsTransactional = true;
                options.IsolationLevel = IsolationLevel.ReadCommitted;
            });
            // 
            services.AddAbpDbContext<AbpDbContext>(p =>
            {
                if (p.DatabaseProvider.ToLower() == EfCoreDatabaseProvider.MySql.ToString().ToLower())
                {
                    if (p.ExistingConnection != null)
                        p.DbContextOptions.UseMySQL(p.ExistingConnection);
                    else
                        p.DbContextOptions.UseMySQL(p.ConnectionString);
                }
                else if (p.DatabaseProvider.ToLower() == EfCoreDatabaseProvider.SqlServer.ToString().ToLower())
                {
                    if (p.ExistingConnection != null)
                        p.DbContextOptions.UseSqlServer(p.ExistingConnection);
                    else
                        p.DbContextOptions.UseSqlServer(p.ConnectionString);
                }
                else if (p.DatabaseProvider.ToLower() == EfCoreDatabaseProvider.Oracle.ToString().ToLower())
                {
                    if (p.ExistingConnection != null)
                        p.DbContextOptions.UseOracle(p.ExistingConnection);
                    else
                        p.DbContextOptions.UseOracle(p.ConnectionString);
                }
                else
                {
                    throw new Exception($"数据库类型配置不正确{p.DatabaseProvider.ToLower()}");
                }
            });
            
            Configure<DataStateOptions>(options =>
            {
                options.DefaultStates[nameof(ISoftDelete)] = true;
            });


        }
    }
   
}

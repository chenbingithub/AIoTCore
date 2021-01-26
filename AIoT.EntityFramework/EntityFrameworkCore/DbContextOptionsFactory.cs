using System;
using System.Collections.Generic;
using AIoT.Core.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AIoT.EntityFramework.EntityFrameworkCore
{
    public static class DbContextOptionsFactory
    {
        public static DbContextOptions<TDbContext> Create<TDbContext>(IServiceProvider serviceProvider)
            where TDbContext : EntityFramework.EntityFrameworkCore.AbpDbContext<TDbContext>
        {
            var creationContext = GetCreationContext<TDbContext>(serviceProvider);

            var context = new EntityFramework.EntityFrameworkCore.AbpDbContextConfigurationContext<TDbContext>(
                creationContext.ConnectionString,
                creationContext.DatabaseProvider,
                serviceProvider,
                creationContext.ConnectionStringName,
                creationContext.ExistingConnection
            );

            var options = GetDbContextOptions<TDbContext>(serviceProvider);

            PreConfigure(options, context);
            Configure(options, context);

            return context.DbContextOptions.Options;
        }

        private static void PreConfigure<TDbContext>(
            EntityFramework.EntityFrameworkCore.AbpDbContextOptions options, EntityFramework.EntityFrameworkCore.AbpDbContextConfigurationContext<TDbContext> context)
            where TDbContext : EntityFramework.EntityFrameworkCore.AbpDbContext<TDbContext>
        {
            foreach (var defaultPreConfigureAction in options.DefaultPreConfigureActions)
            {
                defaultPreConfigureAction.Invoke(context);
            }

            var preConfigureActions = options.PreConfigureActions.GetValueOrDefault(typeof(TDbContext));
            if (!(preConfigureActions == null || preConfigureActions.Count <= 0))
            {
                foreach (var preConfigureAction in preConfigureActions)
                {
                    ((Action<EntityFramework.EntityFrameworkCore.AbpDbContextConfigurationContext<TDbContext>>)preConfigureAction).Invoke(context);
                }
            }
        }

        private static void Configure<TDbContext>(
            EntityFramework.EntityFrameworkCore.AbpDbContextOptions options, EntityFramework.EntityFrameworkCore.AbpDbContextConfigurationContext<TDbContext> context)
            where TDbContext : EntityFramework.EntityFrameworkCore.AbpDbContext<TDbContext>
        {
            var configureAction = options.ConfigureActions.GetValueOrDefault(typeof(TDbContext));
            if (configureAction != null)
            {
                ((Action<EntityFramework.EntityFrameworkCore.AbpDbContextConfigurationContext<TDbContext>>)configureAction).Invoke(context);
            }
            else if (options.DefaultConfigureAction != null)
            {
                options.DefaultConfigureAction.Invoke(context);
            }
            else
            {
                throw new Exception(
                    $"No configuration found for {typeof(DbContext).AssemblyQualifiedName}! Use services.Configure<AbpDbContextOptions>(...) to configure it.");
            }
        }

        private static EntityFramework.EntityFrameworkCore.AbpDbContextOptions GetDbContextOptions<TDbContext>(IServiceProvider serviceProvider)
            where TDbContext : EntityFramework.EntityFrameworkCore.AbpDbContext<TDbContext>
        {
            return serviceProvider.GetRequiredService<IOptions<EntityFramework.EntityFrameworkCore.AbpDbContextOptions>>().Value;
        }

        private static EntityFramework.EntityFrameworkCore.DbContextCreationContext GetCreationContext<TDbContext>(IServiceProvider serviceProvider)
            where TDbContext : EntityFramework.EntityFrameworkCore.AbpDbContext<TDbContext>
        {
            var context = EntityFramework.EntityFrameworkCore.DbContextCreationContext.Current;
            if (context != null)
            {
                return context;
            }

            var connectionStringResolver = serviceProvider.GetRequiredService<IConnectionStringResolver>();
            var connectionStringName = typeof(TDbContext).Name;
            var connectionString = connectionStringResolver.Resolve<TDbContext>();
            var databaseProvider = connectionStringResolver.GetDatabaseProvider();

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception($"没有配置 {connectionStringName} 的数据库连接字符串。");
            }

            return new EntityFramework.EntityFrameworkCore.DbContextCreationContext(
                connectionStringName,
                connectionString,
                databaseProvider
            );
        }
    }
}

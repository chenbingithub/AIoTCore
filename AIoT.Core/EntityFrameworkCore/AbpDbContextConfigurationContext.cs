using System;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AIoT.Core.EntityFrameworkCore
{
    public class AbpDbContextConfigurationContext 
    {
        public IServiceProvider ServiceProvider { get; }

        public string ConnectionString { get; }

        public string ConnectionStringName { get; }
        /// <summary>
        /// 
        /// </summary>
        public string DatabaseProvider { get; }

        public DbConnection ExistingConnection { get; }

        public DbContextOptionsBuilder DbContextOptions { get; protected set; }

        public AbpDbContextConfigurationContext(
            [NotNull] string connectionString,
            [NotNull] string databaseProvider,
            [NotNull] IServiceProvider serviceProvider,
            string connectionStringName,
            DbConnection existingConnection)
        {
            ConnectionString = connectionString;
            DatabaseProvider = connectionString;
            ServiceProvider = serviceProvider;
            ConnectionStringName = connectionStringName;
            ExistingConnection = existingConnection;

            DbContextOptions = new DbContextOptionsBuilder()
                .UseLoggerFactory(serviceProvider.GetRequiredService<ILoggerFactory>());
        }
    }

    public class AbpDbContextConfigurationContext<TDbContext> : AbpDbContextConfigurationContext
        where TDbContext : AbpDbContext<TDbContext>
    {
        public new DbContextOptionsBuilder<TDbContext> DbContextOptions => (DbContextOptionsBuilder<TDbContext>)base.DbContextOptions;

        public AbpDbContextConfigurationContext(
            [NotNull]string connectionString,
            [NotNull] string databaseProvider,
            [NotNull] IServiceProvider serviceProvider,
            string connectionStringName,
             DbConnection existingConnection)
            : base(
                  connectionString,
                  databaseProvider,
                  serviceProvider, 
                  connectionStringName, 
                  existingConnection)
        {
            base.DbContextOptions = new DbContextOptionsBuilder<TDbContext>()
                .UseLoggerFactory(serviceProvider.GetRequiredService<ILoggerFactory>());
        }
    }
}
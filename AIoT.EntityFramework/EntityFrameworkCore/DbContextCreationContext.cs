using System;
using System.Data.Common;
using System.Threading;
using Volo.Abp;

namespace AIoT.EntityFramework.EntityFrameworkCore
{
    public class DbContextCreationContext
    {
        public static DbContextCreationContext Current => _current.Value;
        private static readonly AsyncLocal<DbContextCreationContext> _current = new AsyncLocal<DbContextCreationContext>();

        public string ConnectionStringName { get; }

        public string ConnectionString { get; }
        public string DatabaseProvider { get; }

        public DbConnection ExistingConnection { get; set; }

        public DbContextCreationContext(string connectionStringName, string connectionString, string databaseProvider)
        {
            ConnectionStringName = connectionStringName;
            ConnectionString = connectionString;
            DatabaseProvider = databaseProvider;
        }

        public static IDisposable Use(DbContextCreationContext context)
        {
            var previousValue = Current;
            _current.Value = context;
            return new DisposeAction(() => _current.Value = previousValue);
        }
    }

  
}
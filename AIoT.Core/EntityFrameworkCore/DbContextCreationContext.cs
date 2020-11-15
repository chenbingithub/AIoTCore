using System;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Volo.Abp;

namespace AIoT.Core.EntityFrameworkCore
{
    public class DbContextCreationContext
    {
        public static DbContextCreationContext Current => _current.Value;
        private static readonly AsyncLocal<DbContextCreationContext> _current = new AsyncLocal<DbContextCreationContext>();

        public string ConnectionStringName { get; }

        public string ConnectionString { get; }

        public DbConnection ExistingConnection { get; set; }

        public DbContextCreationContext(string connectionStringName, string connectionString)
        {
            ConnectionStringName = connectionStringName;
            ConnectionString = connectionString;
        }

        public static IDisposable Use(DbContextCreationContext context)
        {
            var previousValue = Current;
            _current.Value = context;
            return new DisposeAction(() => _current.Value = previousValue);
        }
    }

    //public class DisposeAction : IDisposable
    //{
    //    private readonly Action _action;

    //    /// <summary>
    //    /// Creates a new <see cref="DisposeAction"/> object.
    //    /// </summary>
    //    /// <param name="action">Action to be executed when this object is disposed.</param>
    //    public DisposeAction([NotNull] Action action)
    //    {
    //        Check.NotNull(action, nameof(action));

    //        _action = action;
    //    }

    //    public void Dispose()
    //    {
    //        _action();
    //    }
    //}
}
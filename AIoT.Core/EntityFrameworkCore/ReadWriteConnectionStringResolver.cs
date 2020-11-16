using System.Collections.Generic;
using AIoT.Core.DataFilter;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;

namespace AIoT.Core.EntityFrameworkCore
{
    /// <summary>
    /// 
    /// </summary>
    public class DataStateKeys
    {
        /// <summary>
        /// 是否为只读 bool
        /// </summary>
        public const string IsReadonly = "IsReadOnly";

        ///// <summary>
        ///// 软删除
        ///// </summary>
        //public const string SoftDelete = nameof(ISoftDelete);

        /// <summary>
        /// 数据权限开门
        /// </summary>
        public const string DataPermission = nameof(DataPermission);
    }
    /// <summary>
    /// 读写分享连接字符串提供程序
    /// </summary>
    public class ReadWriteConnectionStringResolver : IConnectionStringResolver, ITransientDependency
    {
        private readonly DbConnectionOptions _options;
        private readonly IDataState _state;

        /// <inheritdoc />
        public ReadWriteConnectionStringResolver(IOptions<DbConnectionOptions> options, IDataState state)
        {
            _state = state;
            _options = options.Value;
        }

        /// <inheritdoc />
        public virtual string Resolve<TDbContext>() where TDbContext : DbContext
        {
            var contextName = typeof(TDbContext).Name;
            string con;

            // DbContext Connection String
            if (_state.IsEnabled(DataStateKeys.IsReadonly))
            {
                // 当前是否为只读模式
                con = _options.ConnectionStrings.GetValueOrDefault($"{contextName}{ConnectionStrings.DefaultReadonlyConnectionStringName}");
                if (!string.IsNullOrWhiteSpace(con)) return con;
            }
            con = _options.ConnectionStrings.GetValueOrDefault(contextName);
            if (!string.IsNullOrWhiteSpace(con)) return con;

            // Default Connection String
            if (_state.IsEnabled(DataStateKeys.IsReadonly))
            {
                // 当前是否为只读模式
                con = _options.ConnectionStrings.GetValueOrDefault(ConnectionStrings.DefaultReadonlyConnectionStringName);
                if (!string.IsNullOrWhiteSpace(con)) return con;
            }

            return _options.ConnectionStrings.Default;
        }

        public virtual string GetDatabaseProvider() 
        {
           
            return _options.ConnectionStrings.DatabaseProvider;
        }
    }
}
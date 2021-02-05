using System.Collections.Generic;
using AIoT.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;

namespace AIoT.EntityFramework.EntityFrameworkCore
{
   
    /// <summary>
    /// 读写分享连接字符串提供程序
    /// </summary>
    public class ReadWriteConnectionStringResolver : EntityFramework.EntityFrameworkCore.IConnectionStringResolver, ITransientDependency
    {
        private readonly EntityFramework.EntityFrameworkCore.DbConnectionOptions _options;
        private readonly IDataFilter _dataFilter;
        /// <inheritdoc />
        public ReadWriteConnectionStringResolver(IOptions<EntityFramework.EntityFrameworkCore.DbConnectionOptions> options, IDataFilter dataFilter)
        {
            _dataFilter = dataFilter;
            _options = options.Value;
        }

        /// <inheritdoc />
        public virtual string Resolve<TDbContext>() where TDbContext : DbContext
        {
            var contextName = typeof(TDbContext).Name;
            string con;

            // DbContext Connection String
            if (_dataFilter.IsEnabled<IReadonly>())
            {
                // 当前是否为只读模式
                con = _options.ConnectionStrings.GetValueOrDefault($"{contextName}{EntityFramework.EntityFrameworkCore.ConnectionStrings.DefaultReadonlyConnectionStringName}");
                if (!string.IsNullOrWhiteSpace(con)) return con;
            }
            con = _options.ConnectionStrings.GetValueOrDefault(contextName);
            if (!string.IsNullOrWhiteSpace(con)) return con;

            // Default Connection String
            if (_dataFilter.IsEnabled<IReadonly>())
            {
                // 当前是否为只读模式
                con = _options.ConnectionStrings.GetValueOrDefault(EntityFramework.EntityFrameworkCore.ConnectionStrings.DefaultReadonlyConnectionStringName);
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
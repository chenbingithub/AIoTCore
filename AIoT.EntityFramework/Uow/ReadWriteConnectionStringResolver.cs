using System;
using System.Collections.Generic;
using AIoT.Core;
using AIoT.Core.Data;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;

namespace AIoT.EntityFramework.Uow
{

    /// <summary>
    /// 读写分享连接字符串提供程序
    /// </summary>
    [Dependency(ReplaceServices = true)]
    public class ReadWriteConnectionStringResolver :IConnectionStringResolver, ITransientDependency
    {
        private readonly AbpDbConnectionOptions _options;
        private readonly IDataFilter _dataFilter;
        /// <inheritdoc />
        public ReadWriteConnectionStringResolver(IOptions<AbpDbConnectionOptions> options, IDataFilter dataFilter)
        {
            _dataFilter = dataFilter;
            _options = options.Value;
        }

        /// <inheritdoc />
        public virtual string Resolve(string connectionStringName = null)
        {
            string con = string.Empty;
            //Get module specific value if provided
            if (!connectionStringName.IsNullOrEmpty())
            {
                 con = _options.ConnectionStrings.GetOrDefault(connectionStringName);
                if (!con.IsNullOrEmpty())
                {
                    return con;
                }
            }
            // Default Connection String
            if (_dataFilter.IsEnabled<IReadonly>())
            {
                // 当前是否为只读模式
                con = _options.ConnectionStrings.GetValueOrDefault(ConnectionStrings.DefaultReadonlyConnectionStringName);
                if (!string.IsNullOrWhiteSpace(con)) return con;
            }
            return _options.ConnectionStrings.Default;
        }
    }
}
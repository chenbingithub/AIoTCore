using Microsoft.EntityFrameworkCore;

namespace AIoT.Core.EntityFrameworkCore
{
    /// <summary>
    /// 连接字符串提供程序
    /// </summary>
    public interface IConnectionStringResolver
    {
        /// <summary>
        /// 获取指定名称的连接字符串
        /// </summary>
        string Resolve<TDbContext>() where TDbContext : DbContext;
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        string GetDatabaseProvider();
    }
}

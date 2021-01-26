using Microsoft.EntityFrameworkCore;

namespace AIoT.EntityFramework.EntityFrameworkCore
{
    /// <summary>
    /// 数据库上下文提供程序
    /// </summary>
    public interface IDbContextProvider
    {
        /// <summary>
        /// 获取数据库上下文
        /// </summary>
        TDbContext GetDbContext<TDbContext>() where TDbContext : DbContext;

        TDbContext GetDbContext<TDbContext>(string databaseKey) where TDbContext : DbContext;
    }
}

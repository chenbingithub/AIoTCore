﻿ using AIoT.Core.Enums;
  using Microsoft.EntityFrameworkCore;

  namespace AIoT.Core.EntityFrameworkCore
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

        TDbContext GetDbContext<TDbContext>(EfCoreDatabaseProvider databaseProvider) where TDbContext : DbContext;
    }
}

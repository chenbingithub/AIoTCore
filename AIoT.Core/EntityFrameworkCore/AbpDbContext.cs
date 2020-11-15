using Microsoft.EntityFrameworkCore;

namespace AIoT.Core.EntityFrameworkCore
{
    
    public class AbpDbContext<TDbContext> : DbContext
        where TDbContext : DbContext
    {
        protected AbpDbContext(DbContextOptions<TDbContext> options)
            : base(options)
        {

        }
    }


    public class AbpDbContext : DbContext
    {
        /// <summary>
        /// 实体上下文基类
        /// </summary>
        public AbpDbContext(DbContextOptions options) : base(options)
        {
        }
    }
    
}

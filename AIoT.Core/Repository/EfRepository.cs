using AIoT.Core.Entities;
using AIoT.Core.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AIoT.Core.Repository
{
    /// <summary>
    /// 写库仓储
    /// </summary>
    public class EfRepository<TDbContext, TEntity> :
        EfWriteRepository<TDbContext, TEntity>,
        IRepository<TEntity>
        where TDbContext : DbContext
        where TEntity : class, IEntity
    {
        /// <inheritdoc />
        public EfRepository(IDbContextProvider dbContextProvider) : base(dbContextProvider)
        {
        }
    }

    /// <inheritdoc cref="EfWriteRepository{TDbContext,TEntity}" />
    public class EfRepository<TDbContext, TEntity, TKey> :
        EfWriteRepository<TDbContext, TEntity, TKey>,
        IRepository<TEntity, TKey>
        where TDbContext : DbContext
        where TEntity : class, IEntity<TKey>
    {
        /// <inheritdoc />
        public EfRepository(IDbContextProvider dbContextProvider) : base(dbContextProvider)
        {
        }
    }
}

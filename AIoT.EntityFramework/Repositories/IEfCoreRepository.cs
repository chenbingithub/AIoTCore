using AIoT.Core.Entities;
using AIoT.Core.Repository;
using Microsoft.EntityFrameworkCore;

namespace AIoT.EntityFramework.Repositories
{
    public interface IEfCoreRepository<TEntity> : IRepository<TEntity>
        where TEntity : class, IEntity
    {
        DbContext DbContext { get; }
        

        DbSet<TEntity> DbSet { get; }
        DbContext ReadDbContext { get; }
        DbSet<TEntity> ReadDbSet { get; }
    }

    public interface IEfCoreRepository<TEntity, TKey> : IEfCoreRepository<TEntity>, IRepository<TEntity, TKey>
        where TEntity : class, IEntity<TKey>
    {

    }
}
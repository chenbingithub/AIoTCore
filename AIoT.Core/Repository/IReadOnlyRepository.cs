using System;
using System.Linq;
using System.Linq.Expressions;
using AIoT.Core.Entities;
using AIoT.Core.Linq;

namespace AIoT.Core.Repository
{
    public interface IReadOnlyRepository<TEntity> : IQueryable<TEntity>, IReadOnlyBasicRepository<TEntity>
        where TEntity : class, IEntity
    {
        IAsyncQueryableExecuter AsyncExecuter { get; }

        IQueryable<TEntity> WithDetails();

        IQueryable<TEntity> Including(params Expression<Func<TEntity, object>>[] propertySelectors);
    }

    public interface IReadOnlyRepository<TEntity, TKey> : IReadOnlyRepository<TEntity>, IReadOnlyBasicRepository<TEntity, TKey>
        where TEntity : class, IEntity<TKey>
    {

    }
}

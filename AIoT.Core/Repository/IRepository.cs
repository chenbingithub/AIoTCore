using System.Diagnostics.CodeAnalysis;
using AIoT.Core.Entities;

namespace AIoT.Core.Repository
{
    /// <summary>
    /// 仓储接口基类
    /// </summary>
    public interface IRepository
    {

    }

    /// <summary>
    /// 仓储接口基类
    /// </summary>
    [SuppressMessage("ReSharper", "RedundantExtendsListEntry")]
    public interface IRepository<TEntity> :
        IRepository,
        IReadRepository<TEntity>,
        IWriteRepository<TEntity>
        where TEntity : class, IEntity
    {

    }

    /// <summary>
    /// 仓储接口基类
    /// </summary>
    [SuppressMessage("ReSharper", "RedundantExtendsListEntry")]
    public interface IRepository<TEntity, in TKey> : 
        IRepository<TEntity>, 
        IReadRepository<TEntity, TKey>, 
        IWriteRepository<TEntity, TKey>
        where TEntity : class, IEntity<TKey>
    {

    }
}

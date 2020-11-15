using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AIoT.Core.Entities;

namespace AIoT.Core.Repository
{
    /// <summary>
    /// 操作仓储
    /// </summary>
    public interface IWriteRepository<TEntity> : IReadRepository<TEntity>, IQueryable<TEntity>
        where TEntity : class, IEntity
    {
        /// <summary>
        /// 新增实体
        /// </summary>
        Task<TEntity> InsertAsync(TEntity entity);

        

        /// <summary>
        /// 更新实体
        /// </summary>
        Task<TEntity> UpdateAsync(TEntity entity);

        /// <summary>
        /// 删除实体
        /// </summary>
        Task DeleteAsync(params object[] keys);

        /// <summary>
        /// 删除实体
        /// </summary>
        Task DeleteAsync(TEntity entity);

        /// <summary>
        /// 删除实体
        /// </summary>
        Task DeleteAsync(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// 提交修改
        /// </summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取实体，如果本地有则直接返回，否则从数据库查询
        /// </summary>
        Task<TEntity> GetAsync(params object[] keys);

        /// <summary>
        /// 查找实体，从数据库查询
        /// </summary>
        Task<TEntity> FirstOrDefaultAsync(params object[] keys);

        /// <summary>
        /// 查询符合条件的第一个实体或默认值
        /// </summary>
        Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// 查询符合条件的唯一实体，如果有多个则会抛出异常
        /// </summary>
        Task<TEntity> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// 获取所有实体
        /// </summary>
        Task<List<TEntity>> GetAllListAsync();

        /// <summary>
        /// 获取符合条件的所有实体
        /// </summary>
        Task<List<TEntity>> GetAllListAsync(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// 获取 <see cref="IQueryable{TEntity}"/>
        /// </summary>
        IQueryable<TEntity> Including(params Expression<Func<TEntity, object>>[] propertySelectors);
    }


    /// <summary>
    /// 操作仓储
    /// </summary>
    public interface IWriteRepository<TEntity, in TKey> : IWriteRepository<TEntity>, IReadRepository<TEntity, TKey>
        where TEntity : class, IEntity<TKey>
    {
        /// <summary>
        /// 获取实体，如果本地有则直接返回，否则从数据库查询
        /// </summary>
        Task<TEntity> GetAsync(TKey key);

        /// <summary>
        /// 查找实体
        /// </summary>
        Task<TEntity> FirstOrDefaultAsync(TKey key);

        /// <summary>
        /// 删除实体
        /// </summary>
        Task DeleteAsync(TKey key);
    }
}

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AIoT.Core.Entities;

namespace AIoT.Core.Repository
{
    /// <summary>
    /// 只读查询库
    /// </summary>
    public interface IReadRepository<TEntity> : IRepository, IQueryable<TEntity>
        where TEntity : class, IEntity
    {
        /// <summary>
        /// 只读查询
        /// </summary>
        IQueryable<TEntity> Readonly();

        /// <summary>
        /// 获取主键查询条件
        /// </summary>
        Expression<Func<TEntity, bool>> GetKeyExpression(params object[] keys);

        ///// <summary>
        ///// 是否存在
        ///// </summary>
        //Task<bool> AnyAsync(IQuery query);

        /// <summary>
        /// 是否存在
        /// </summary>
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate);

        ///// <summary>
        ///// 查询单个数据
        ///// </summary>
        //Task<TDto> FirstOrDefaultAsync<TDto>(params object[] keys);

        ///// <summary>
        ///// 查询单个数据
        ///// </summary>
        //Task<TDto> FirstOrDefaultAsync<TDto>(IQuery query);

        ///// <summary>
        ///// 查询单个数据
        ///// </summary>
        ///// <example>
        ///// <code>
        ///// <![CDATA[
        ///// FirstOrDefaultAsyn<User>(p => true, "Id");
        ///// FirstOrDefaultAsyn<User>(p => true, "Id DESC");
        ///// FirstOrDefaultAsyn<User>(p => true, "Status, CreationTime DESC");
        ///// ]]>
        ///// </code>
        ///// </example>
        //Task<TDto> FirstOrDefaultAsync<TDto>(Expression<Func<TEntity, bool>> query, string sort = null);

        ///// <summary>
        ///// 查询所有数据
        ///// </summary>
        //Task<List<TDto>> GetAllListAsync<TDto>(IQuery query, ISortInfo sortInfo = null, string defaultSort = null);

        ///// <summary>
        ///// 查询单个数据
        ///// </summary>
        //Task<List<TDto>> GetAllListAsync<TDto>(Expression<Func<TEntity, bool>> query, ISortInfo sortInfo = null, string defaultSort = null);

        ///// <summary>
        ///// 查询单个数据
        ///// </summary>
        //Task<List<TDto>> GetAllListAsync<TDto>(ISortInfo sortInfo = null, string defaultSort = null);

        ///// <summary>
        ///// 分页查询数据
        ///// </summary>
        //Task<PageResult<TDto>> GetByPageAsync<TDto>(IPageQuery query, string defaultSort = null);


        //#region Aggregates

        /// <summary>
        /// 查询所有数据数量
        /// </summary>
        Task<int> CountAsync();

        /// <summary>
        /// 查询符合条件的数据数量
        /// </summary>
        Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate);

       
    }

    /// <summary>
    /// 只读查询库
    /// </summary>
    public interface IReadRepository<TEntity, in TKey> : IReadRepository<TEntity>
        where TEntity : class, IEntity<TKey>
    {
        /// <summary>
        /// 获取主键查询条件
        /// </summary>
        Expression<Func<TEntity, bool>> GetKeyExpression(TKey key);

        ///// <summary>
        ///// 根据Id获取数据
        ///// </summary>
        //Task<TDto> FirstOrDefaultAsync<TDto>(TKey key);
    }
}

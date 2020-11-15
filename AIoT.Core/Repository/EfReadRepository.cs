using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AIoT.Core.DataFilter;
using AIoT.Core.Entities;
using AIoT.Core.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AIoT.Core.Repository
{
    /// <summary>
    /// 读库仓储
    /// </summary>
    public class EfReadRepository<TDbContext, TEntity> : EfRepositoryBase, IReadRepository<TEntity>, IEnumerable where TDbContext : AbpDbContext
        where TEntity : class, IEntity
    {
        #region 构造函数、私有字段

        private readonly IDbContextProvider _dbContextProvider;

        public IDataState DataState { protected get; set; } = NullDataState.Instanse;

        public ICancellationTokenAccessor CancellationTokenAccessor { protected get; set; }

        /// <summary>
        /// 获取 <see cref="ICancellationTokenAccessor.Token"/>
        /// </summary>
        protected CancellationToken CancellationToken => CancellationTokenAccessor?.Token ?? default;

        /// <summary>
        /// 数据上下文
        /// </summary>
        protected virtual TDbContext Context => _dbContextProvider.GetDbContext<TDbContext>();

        /// <summary>
        /// 实体数据集
        /// </summary>
        protected virtual DbSet<TEntity> Table => Context.Set<TEntity>();

        /// <summary>
        /// 读库数据上下文
        /// </summary>
        protected virtual TDbContext ReadContext
        {
            get
            {
                using (DataState.Enable(DataStateKeys.IsReadonly))
                {
                    return _dbContextProvider.GetDbContext<TDbContext>();
                }
            }
        }

        /// <summary>
        /// 读库实体数据集
        /// </summary>
        protected virtual DbSet<TEntity> ReadTable => ReadContext.Set<TEntity>();


        /// <summary>只读查询库基类</summary>
        public EfReadRepository(IDbContextProvider dbContextProvider)
        {
            _dbContextProvider = dbContextProvider;
        }

        #endregion

   

        #region IReadonlyRepository

        /// <inheritdoc />
        public virtual IQueryable<TEntity> Readonly()
        {
            return ReadTable.AsNoTracking();
        }

        /// <inheritdoc />
        public Expression<Func<TEntity, bool>> GetKeyExpression(params object[] keys)
            => CreateEqualityExpressionForKeys<TEntity>(Context, keys);

       

        /// <summary>
        /// 是否存在
        /// </summary>
        public virtual async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await Table.AnyAsync(predicate, CancellationToken);
        }
        ///// <summary>
        ///// 是否存在
        ///// </summary>
        //public virtual async Task<bool> AnyAsync(IQuery query)
        //{
        //    var predicate = query.GetQueryExpression<TEntity>();
        //    return await AnyAsync(predicate);
        //}
        ///// <inheritdoc />
        //public async Task<TDto> FirstOrDefaultAsync<TDto>(params object[] keys)
        //{
        //    var idFilter = CreateEqualityExpressionForKeys<TEntity>(Context, keys);

        //    var data = Table.Where(idFilter);
        //    var map = typeof(TDto) == typeof(TEntity)
        //        ? (IQueryable<TDto>)data
        //        : data.ProjectTo<TDto>();

        //    return await map.FirstOrDefaultAsync(CancellationToken);
        //}

        ///// <summary>
        ///// 查询单个数据
        ///// </summary>
        //public virtual async Task<TDto> FirstOrDefaultAsync<TDto>(IQuery query)
        //{
        //    return await Table.FirstOrDefaultAsync<TEntity, TDto>(query, CancellationToken);
        //}

        ///// <summary>
        ///// 查询单个数据
        ///// </summary>
        //public virtual async Task<TDto> FirstOrDefaultAsync<TDto>(Expression<Func<TEntity, bool>> query, string sort = null)
        //{
        //    var data = Table.Where(query).ProjectTo<TDto>();
        //    if (!string.IsNullOrEmpty(sort))
        //        data = data.OrderBy(sort);

        //    return await data.FirstOrDefaultAsync(CancellationToken);
        //}

        ///// <summary>
        ///// 查询多条数据
        ///// </summary>
        //public virtual async Task<List<TDto>> GetAllListAsync<TDto>(IQuery query, ISortInfo sortInfo = null, string defaultSort = null)
        //{
        //    return await Table.ToListAsync<TEntity, TDto>(query, sortInfo ?? query as ISortInfo, defaultSort, CancellationToken);
        //}

        ///// <summary>
        ///// 查询多条数据
        ///// </summary>
        //public virtual async Task<List<TDto>> GetAllListAsync<TDto>(Expression<Func<TEntity, bool>> query, ISortInfo sortInfo = null, string defaultSort = null)
        //{
        //    return await Table.ToListAsync<TEntity, TDto>(query, sortInfo, defaultSort, CancellationToken);
        //}

        ///// <summary>
        ///// 查询多条数据
        ///// </summary>
        //public virtual async Task<List<TDto>> GetAllListAsync<TDto>(ISortInfo sortInfo = null, string defaultSort = null)
        //{
        //    return await Table.ToListAsync<TEntity, TDto>(sortInfo, defaultSort, CancellationToken);
        //}

        ///// <summary>
        ///// 分页查询数据
        ///// </summary>
        //public virtual async Task<PageResult<TDto>> GetByPageAsync<TDto>(IPageQuery query, string defaultSort = null)
        //{
        //    return await Table.ToPageResultAsync<TEntity, TDto>(query, defaultSort, cancellationToken: CancellationToken);
        //}

        /// <inheritdoc />
        public virtual async Task<int> CountAsync()
        {
            return await Table.CountAsync(CancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await Table.CountAsync(predicate, CancellationToken);
        }

        ///// <summary>
        ///// 查出单个字段之和
        ///// </summary>      
        //public virtual async Task<decimal> SumAsync(Expression<Func<TEntity, decimal>> predicate)
        //{
        //    return await Table.SumAsync(predicate);
        //}

        ///// <inheritdoc />
        //public virtual async Task<long> LongCountAsync()
        //{
        //    return await Table.LongCountAsync();
        //}

        ///// <inheritdoc />
        //public virtual async Task<long> LongCountAsync(Expression<Func<TEntity, bool>> predicate)
        //{
        //    return await Table.LongCountAsync(predicate);
        //}

        #endregion

        #region IQueryable

        /// <inheritdoc />
        public virtual IEnumerator<TEntity> GetEnumerator() =>
            throw new NotSupportedException("Plase use .ToListAsync() before enumerate");

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public virtual Type ElementType => ((IQueryable<TEntity>)Table).ElementType;

        /// <inheritdoc />
        public virtual Expression Expression => ((IQueryable<TEntity>)Table).Expression;

        /// <inheritdoc />
        public virtual IQueryProvider Provider => ((IQueryable<TEntity>)Table).Provider;

        #endregion
    }


    /// <summary>
    /// 读库仓储
    /// </summary>
    public class EfReadRepository<TDbContext, TEntity, TKey> : EfReadRepository<TDbContext, TEntity>, IReadRepository<TEntity, TKey>
        where TDbContext : AbpDbContext
        where TEntity : class, IEntity<TKey>
    {
        /// <inheritdoc />
        public EfReadRepository(IDbContextProvider dbContextProvider, IDataState dataState) : base(dbContextProvider)
        {
        }

        /// <inheritdoc />
        public Expression<Func<TEntity, bool>> GetKeyExpression(TKey key)
            => CreateEqualityExpressionForKeys<TEntity, TKey>(key);

        ///// <summary>
        ///// 根据Id获取数据
        ///// </summary>
        //public virtual async Task<TDto> FirstOrDefaultAsync<TDto>(TKey key)
        //{
        //    var idFilter = CreateEqualityExpressionForKeys<TEntity, TKey>(key);

        //    var data = Table.Where(idFilter);
        //    var map = typeof(TDto) == typeof(TEntity)
        //        ? (IQueryable<TDto>)data
        //        : data.ProjectTo<TDto>();

        //    return await map.FirstOrDefaultAsync(CancellationToken);
        //}
    }
}

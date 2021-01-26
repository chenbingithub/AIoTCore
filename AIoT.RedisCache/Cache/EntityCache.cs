using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

//using StackExchange.Profiling;

namespace AIoT.Core.Cache
{
    /// <summary>
    /// 实体缓存基类
    /// </summary>
    public abstract class EntityCache
    {
        /// <summary>
        /// 缓存名称
        /// </summary>
        public const string DefaultCacheName = "EntityCache";
    }

    /// <summary>
    /// 使用分布式缓存实现 <see cref="IEntityCache{TCacheItem}"/>
    /// </summary>
    public abstract class EntityCache<TEntity, TCacheItem, TKey> : EntityCache,
        IEntityCache<TCacheItem, TKey>,
        ILocalEventHandler<EntityChangedEventData<TEntity>>
        where TEntity : class, IEntity<TKey>
    {
        /// <summary>
        /// 使用分布式缓存实现 <see cref="IEntityCache{TCacheItem}"/>
        /// </summary>
        protected EntityCache(IRepository<TEntity, TKey> repository, ICacheManager cacheManager, string cacheName = DefaultCacheName)
        {
            CacheName = cacheName ?? DefaultCacheName;
            Repository = repository;
            InternalCache = cacheManager.GetCache<TCacheItem>(CacheName);
        }

        /// <summary>
        /// 缓存名称
        /// </summary>
        protected string CacheName { get; }

        /// <summary>
        /// 缓存服务
        /// </summary>
        public ICache<TCacheItem> InternalCache { get; }

        /// <summary>
        /// 仓储服务
        /// </summary>
        protected IRepository<TEntity, TKey> Repository { get; }

        /// <summary>
        /// 工作单元
        /// </summary>
        public IUnitOfWorkManager UnitOfWorkManager { protected get; set; }
        
        /// <inheritdoc />
        [UnitOfWork]
        public async ValueTask<TCacheItem> GetAsync(TKey id)
        {
            //using (MiniProfiler.Current.Step($"{GetType().Name}.GetAsync({id})"))
            {
                return await InternalCache.GetOrAddAsync(GetCacheKey(id), async () => await GetFromDbAsync(id));
            }
        }
        /// <summary>
        /// 获取缓存Key
        /// 生成规则 Entity:CacheItem:id
        /// </summary>
        protected virtual string GetCacheKey(object[] keys)
        {
            return $"{typeof(TEntity).Name}:{typeof(TCacheItem).Name}:{string.Join(':', keys)}";
        }
        #region ILocalEventHandler<IEntityChangedEventData<TEntity>>

        /// <inheritdoc />
        async Task ILocalEventHandler<EntityChangedEventData<TEntity>>.HandleEventAsync(
            EntityChangedEventData<TEntity> eventData)
        {
            if (eventData.Entity != null)
            {
                await OnEntityChanged(eventData.Entity);
            }
        }

        /// <summary>
        /// 订阅实体变更事件，清除缓存数据
        /// </summary>
        protected virtual async Task OnEntityChanged(TEntity entity)
        {
            await InternalCache.RemoveAsync(GetCacheKey(entity.GetKeys()));
        }

        #endregion

        /// <summary>
        /// 从数据库获取缓存数据
        /// </summary>
        protected virtual async Task<TCacheItem> GetFromDbAsync(TKey id)
        {
            //using (UnitOfWorkManager.Current.DisableFilter(DataFilters.AgencyPermission, DataFilters.ParkPermission))
            {
                var cacheItem = await Repository.Where(CreateGetEntityByIdExpression(id))
                    .ProjectTo<TCacheItem>().FirstOrDefaultAsync();
                return cacheItem;
            }
        }

        /// <summary>
        /// 获取缓存Key
        /// 生成规则 Entity:CacheItem:id
        /// </summary>
        protected virtual string GetCacheKey(TKey id)
        {
            return $"{typeof(TEntity).Name}:{typeof(TCacheItem).Name}:{id}";
        }

        /// <summary>
        /// 创建 p => p.Id == id 的表达式
        /// </summary>
        protected static Expression<Func<TEntity, bool>> CreateGetEntityByIdExpression(TKey id)
        {
            var p = Expression.Parameter(typeof(TEntity), "p");

            var body = Expression.Equal(
                Expression.Property(p, "Id"), 
                Expression.Constant(id, typeof(TKey)));

            return Expression.Lambda<Func<TEntity, bool>>(body, p);
        }
    }

    /// <summary>
    /// 使用分布式缓存实现 <see cref="IEntityCache{TCacheItem}"/>
    /// </summary>
    public abstract class EntityCache<TEntity, TCacheItem> : EntityCache<TEntity, TCacheItem, int>
        where TEntity : class, IEntity<int>
    {
        /// <inheritdoc />
        protected EntityCache(IRepository<TEntity, int> repository, ICacheManager cacheManager, string cacheName = DefaultCacheName) 
            : base(repository, cacheManager, cacheName)
        {
        }
    }
}

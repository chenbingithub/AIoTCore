using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AIoT.Core.Entities;
using AIoT.Core.EventBus.Events;
using AIoT.Core.EventBus.Local;
using AIoT.Core.Repository;
using AIoT.Core.Uow;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.DependencyInjection;

//using StackExchange.Profiling;

namespace AIoT.RedisCache.Cache
{
    /// <summary>
    /// 使用分布式缓存实现 <see cref="IEntityCache{TCacheItem}"/>
    /// </summary>
    public abstract class EntityListCache<TEntity, TCacheItem, TKey> : 
        EntityCache,
        IEntityListCache<TCacheItem, TKey>,
        ILocalEventHandler<EntityChangedEventData<TEntity>>
        where TEntity : class, IEntity<TKey>
    {
        /// <summary>
        /// 使用分布式缓存实现 <see cref="IEntityListCache{TCacheItem}"/>
        /// </summary>
        protected EntityListCache(IRepository<TEntity, TKey> repository, ICacheManager cacheManager)
        {
            CacheName = DefaultCacheName;
            Repository = repository;
            InternalCache = cacheManager.GetListCache<TCacheItem,TKey>(CacheName);
        }

        /// <summary>
        /// 缓存名称
        /// </summary>
        protected string CacheName { get; }

        /// <summary>
        /// 缓存服务
        /// </summary>
        public IListCache<TCacheItem, TKey> InternalCache { get; }

        /// <summary>
        /// 仓储服务
        /// </summary>
        protected IRepository<TEntity, TKey> Repository { get; }



        /// <inheritdoc />
        [UnitOfWork]
        public async ValueTask<TCacheItem> GetAsync(TKey id)
        {
                await GetFromCacheOrDbAsync();
                return await InternalCache.GetAsync(GetCacheKey(), id);
        }

        /// <inheritdoc />
        [UnitOfWork]
        public async ValueTask<IEnumerable<TCacheItem>> GetAllAsync()
        {
                await GetFromCacheOrDbAsync();
                return await InternalCache.GetAllAsync(GetCacheKey());
        }

        /// <inheritdoc />
        [UnitOfWork]
        public async ValueTask<IEnumerable<TCacheItem>> GetAllAsync(IEnumerable<TKey> ids)
        {
            
                var data = await GetFromCacheOrDbAsync();
                if (data == null) return default;

                return await InternalCache.GetAllAsync(GetCacheKey(), ids);
        }

        /// <summary>
        /// 从缓存获取数据
        /// </summary>
        protected virtual async Task<IReadOnlyDictionary<TKey, TCacheItem>> GetFromCacheOrDbAsync()
        {
            return await InternalCache.GetOrAddAsync(GetCacheKey(), async () => await GetFromDbAsync());
        }

        /// <summary>
        /// 从数据库获取缓存数据
        /// </summary>
        protected virtual async Task<IReadOnlyDictionary<TKey, TCacheItem>> GetFromDbAsync()
        {
            var cacheItems = await Repository.ProjectTo<TCacheItem>()
                .ToDictionaryAsync(GetKeyByCacheItem);
            return cacheItems;
        }

        /// <summary>
        /// 从缓存实体中获取主键
        /// </summary>
        protected abstract TKey GetKeyByCacheItem(TCacheItem data);

        /// <summary>
        /// 获取缓存Key
        /// 生成规则 Entity:CacheItem:id
        /// </summary>
        protected virtual string GetCacheKey()
        {
            return $"{typeof(TEntity).Name}:{typeof(TCacheItem).Name}";
        }




        /// <summary>
        /// 订阅实体变更事件，清除缓存数据
        /// </summary>
        protected virtual async Task OnEntityChanged(TEntity entity)
        {
            await InternalCache.RemoveAsync(GetCacheKey());
        }

        /// <summary>
        /// 订阅实体变更事件，清除缓存数据
        /// </summary>
        public async Task HandleEventAsync(EntityChangedEventData<TEntity> eventData, CancellationToken cancellation)
        {
            if (eventData.Entity != null)
            {
                await OnEntityChanged(eventData.Entity);
            }
        }

      
    }

    /// <summary>
    /// 使用分布式缓存实现 <see cref="IEntityCache{TCacheItem}"/>
    /// </summary>
    public abstract class EntityListCache<TEntity, TCacheItem> :
        EntityListCache<TEntity, TCacheItem, int>
        where TEntity : class, IEntity<int>
    {
        /// <inheritdoc />
        protected EntityListCache(IRepository<TEntity, int> repository, ICacheManager cacheManager)
            : base(repository, cacheManager)
        {
        }
    }
}

using AIoT.Core.Cache;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Volo.Abp.DependencyInjection;

namespace AIoT.RedisCache.Cache.Internal
{
    /// <summary>
    /// 缓存管理
    /// </summary>
    public class CacheManager : ICacheManager, ISingletonDependency
    {
        private readonly IOptions<CacheOptions> _config;
        private readonly ICacheStorage _cacheStorage;

        /// <summary>
        /// 缓存管理
        /// </summary>
        public CacheManager(IOptions<CacheOptions> config, ICacheStorage cacheStorage)
        {
            _cacheStorage = cacheStorage;
            _config = config;
        }

        /// <inheritdoc />
        public ICache<TData> GetCache<TData>(string cacheName = null)
        {
            return new Cache<TData>(_cacheStorage, _config, cacheName ?? CacheOptions.DefaultCacheName);
        }

        /// <inheritdoc />
        public IListCache<TData, TId> GetListCache<TData, TId>(string cacheName = null)
        {
            return new ListCache<TData, TId>(_cacheStorage, _config, cacheName ?? CacheOptions.DefaultCacheName);
        }


    }
}

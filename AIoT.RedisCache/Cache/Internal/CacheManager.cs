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
        private readonly CacheOptions _config;
        private readonly IConnectionMultiplexer _redis;
        private readonly IMemoryCache _memoryCache;

        /// <summary>
        /// 缓存管理
        /// </summary>
        public CacheManager(IOptions<CacheOptions> config, IConnectionMultiplexer redis, IMemoryCache memoryCache)
        {
            _config = config.Value;
            _redis = redis;
            _memoryCache = memoryCache;
        }

        /// <inheritdoc />
        public ICache<TData> GetCache<TData>(string cacheName = null)
        {
            return new Cache<TData>(_memoryCache, _redis, _config, cacheName ?? CacheBase.DefaultName);
        }

        /// <inheritdoc />
        public IListCache<TId, TData> GetListCache<TId, TData>(string cacheName = null)
        {
            return new ListCache<TId, TData>(_memoryCache, _redis, _config, cacheName ?? CacheBase.DefaultName);
        }
    }
}

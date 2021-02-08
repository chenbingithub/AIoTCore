using System;
using Microsoft.Extensions.Caching.Memory;
using StackExchange.Redis;

namespace AIoT.RedisCache.Cache.Internal
{
    /// <summary>
    /// 缓存基类
    /// </summary>
    public abstract class CacheBase
    {
        #region 常量值

        /// <summary>
        /// Field 绝对过期时间
        /// </summary>
        public const string AbsoluteExpireKey = "__absexp__";

        /// <summary>
        /// Field 滑动过期时间
        /// </summary>
        public const string SlidingExpireKey = "__sldexp__";

        /// <summary>
        /// Field 缓存数据
        /// </summary>
        public const string DataKey = "__data__";

        /// <summary>
        /// Channel 缓存过期
        /// </summary>
        public const string CacheExpiredChannel = "DistributedCache.CacheExpired";

        /// <summary>
        /// Channel 缓存更新
        /// </summary>
        public const string CacheUpdatedChannel = "DistributedCache.CacheUpdated";

        /// <summary>
        /// Meta 无过期时间
        /// </summary>
        public const long NotPresent = -1;
        
        /// <summary>
        /// 默认缓存名称
        /// </summary>
        public const string DefaultName = "Default";

        #endregion
        
   

        /// <summary>
        /// 内存缓存
        /// </summary>
        protected IMemoryCache MemoryCache { get; }

        /// <summary>
        /// Redis缓存
        /// </summary>
        protected IDatabase RedisCache { get; }

        /// <summary>
        /// 缓存配置
        /// </summary>
        protected CacheEntryOptions Options { get; }

        /// <summary>
        /// 全局配置
        /// </summary>
        protected CacheOptions Config { get; }

        /// <summary>
        /// 缓存名称
        /// </summary>
        public string CacheName { get; }

        /// <inheritdoc />
        protected CacheBase(IMemoryCache memoryCache, IConnectionMultiplexer redis, CacheOptions config, string cacheName = null)
        {
            CacheName = cacheName ?? DefaultName;
            Config = config ?? throw new ArgumentNullException(nameof(config));
            Options = Config.GetOrDefaultOption(CacheName);

            RedisCache = redis.GetDatabase(config.Db);
            MemoryCache = memoryCache;
        }
        
        /// <summary>
        /// 获取缓存Key
        /// </summary>
        protected virtual string GetCacheKey(string key)
        {
            return $"{Config.Prefix}:{CacheName}:{key}";
        }
    }
}
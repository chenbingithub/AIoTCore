using System;
using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
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
        /// Meta 无过期时间
        /// </summary>
        public const long NotPresent = -1;

        #endregion

        /// <summary>
        /// 缓存互斥锁
        /// </summary>
        public static readonly ConcurrentDictionary<string, AsyncLock> Locks = new ConcurrentDictionary<string, AsyncLock>();

        /// <summary>
        /// 缓存存储服务
        /// </summary>
        public ICacheStorage CacheStorage { get; }

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
        protected CacheEntryConfigOptions Options { get; }

        /// <summary>
        /// 全局配置
        /// </summary>
        protected CacheOptions Config { get; }

        /// <summary>
        /// 缓存名称
        /// </summary>
        public string CacheName { get; }

        /// <summary>
        /// 缓存策略
        /// </summary>
        public CacheStoragePolicy StoragePolicy { get; set; }

        /// <inheritdoc cref="CacheBase" />
        protected CacheBase(ICacheStorage cacheStorage, IOptions<CacheOptions> config, string cacheName = CacheOptions.DefaultCacheName)
        {
            CacheName = cacheName ?? CacheOptions.DefaultCacheName;
            Config = config.Value;

            CacheStorage = cacheStorage;
            RedisCache = cacheStorage.GetRedisCache();
            MemoryCache = cacheStorage.GetMemoryCache();
            Options = Config.GetOrDefaultOption(CacheName);

            StoragePolicy = Options.StoragePolicy;
        }

        /// <summary>
        /// 获取缓存Key
        /// </summary>
        protected virtual string GetCacheKey(string key)
        {
            return $"{Config.Prefix}:{key}";
        }
    }
}
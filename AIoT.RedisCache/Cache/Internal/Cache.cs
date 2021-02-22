using System;
using System.Threading.Tasks;
using AIoT.Core.Cache;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace AIoT.RedisCache.Cache.Internal
{
   
    /// <summary>
    /// 分布式缓存实现
    /// </summary>
    public class Cache<TData> : CacheBase, ICache<TData>
    {
        #region 构造函数、私有字段

        /// <inheritdoc />
        public Cache(ICacheStorage cacheStorage, IOptions<CacheOptions> config)
            : this(cacheStorage, config, CacheOptions.DefaultCacheName)
        {
        }

        /// <inheritdoc />
        public Cache(ICacheStorage cacheStorage, IOptions<CacheOptions> config, string cacheName)
            : base(cacheStorage, config, cacheName)
        {
        }

        #endregion


        #region ICache<TData>

        /// <inheritdoc />
        public virtual async ValueTask<TData> GetAsync(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            var rs = await InternalGetAsync(key);
            return rs.Data;
        }

        /// <inheritdoc />
        public virtual async ValueTask<TData> GetOrAddAsync(string key, Func<Task<TData>> factory,
            Func<CacheEntryOptions> optionsFactory = null)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            var (exists, data) = await InternalGetAsync(key);
            if (exists) return data;

            try
            {
                using (await Locks.GetOrAdd(key, p => new AsyncLock()).LockAsync())
                {
                    (exists, data) = await InternalGetAsync(key);
                    if (exists) return data;

                    var value = await factory();
                    if (value != null)
                    {
                        await SetAsync(key, value, optionsFactory?.Invoke());
                    }

                    return value;
                }
            }
            finally
            {
                Locks.TryRemove(key, out _);
            }
        }

        /// <inheritdoc />
        public virtual async Task SetAsync(string key, TData value, CacheEntryOptions options = null)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (options == null) options = Options;

            if (StoragePolicy.HasFlag(CacheStoragePolicy.Redis))
            {
                await SetToRedisAsync(key, value, options);

                if (StoragePolicy.HasFlag(CacheStoragePolicy.Memory))
                {
                    MemoryCache.Remove(GetCacheKey(key));
                }
            }
            else if (StoragePolicy.HasFlag(CacheStoragePolicy.Memory))
            {
                var cacheKey = GetCacheKey(key);

                var absoluteExpire = DateTimeOffset.UtcNow + options.AbsoluteExpire;
                var meta = new CacheMeta
                {
                    AbsoluteExpire = absoluteExpire,
                    SlidingExpire = options.SlidingExpire,
                };

                // 缓存到本地内存
                var cachePolicy = new MemoryCacheEntryOptions()
                {
                    AbsoluteExpiration = absoluteExpire,
                    SlidingExpiration = options.SlidingExpire,
                };
                MemoryCache.Set(cacheKey, new LocalCacheEntry<TData>(value, meta), cachePolicy);
            }
        }


        /// <inheritdoc />
        public virtual async Task<bool> RefreshAsync(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            var cacheKey = GetCacheKey(key);
            CacheMeta meta;

            if (StoragePolicy.HasFlag(CacheStoragePolicy.Memory) &&
                MemoryCache.Get(cacheKey) is LocalCacheEntry<TData> localCache)
            {
                meta = localCache.Meta;
            }
            else if (StoragePolicy.HasFlag(CacheStoragePolicy.Redis))
            {
                meta = await GetMetaFromRedisAsync(key);
            }
            else
            {
                return false;
            }

            if (StoragePolicy.HasFlag(CacheStoragePolicy.Redis))
            {
                await CheckAndRefreshRedisSlidingExpireAsync(meta, cacheKey);
            }

            return true;
        }

        /// <inheritdoc />
        public virtual async Task<bool> RemoveAsync(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            var rs = false;

            var cacheKey = GetCacheKey(key);
            if (StoragePolicy.HasFlag(CacheStoragePolicy.Redis))
            {
                rs = await RedisCache.KeyDeleteAsync(cacheKey);
                await CacheStorage.PublishCacheExpiredAsync(cacheKey);
            }

            if (StoragePolicy.HasFlag(CacheStoragePolicy.Memory))
            {
                MemoryCache.Remove(cacheKey);
                rs = true;
            }
            return rs;
        }

        #endregion

        #region ISyncCache<TData>

        /// <inheritdoc />
        public virtual TData Get(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            var rs = InternalGet(key);
            return rs.Data;
        }

        /// <inheritdoc />
        public virtual TData GetOrAdd(string key, Func<TData> factory,
            Func<CacheEntryOptions> optionsFactory = null)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            var (exists, data) = InternalGet(key);
            if (exists) return data;

            try
            {
                using (Locks.GetOrAdd(key, p => new AsyncLock()).Lock())
                {
                    (exists, data) = InternalGet(key);
                    if (exists) return data;

                    var value = factory();
                    if (value != null)
                    {
                        Set(key, value, optionsFactory?.Invoke());
                    }

                    return value;
                }
            }
            finally
            {
                Locks.TryRemove(key, out _);
            }
        }

        /// <inheritdoc />
        public virtual void Set(string key, TData value, CacheEntryOptions options = null)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (options == null) options = Options;

            if (StoragePolicy.HasFlag(CacheStoragePolicy.Redis))
            {
                SetToRedis(key, value, options);

                if (StoragePolicy.HasFlag(CacheStoragePolicy.Memory))
                {
                    MemoryCache.Remove(GetCacheKey(key));
                }
            }
            else if (StoragePolicy.HasFlag(CacheStoragePolicy.Memory))
            {
                var cacheKey = GetCacheKey(key);

                var absoluteExpire = DateTimeOffset.UtcNow + options.AbsoluteExpire;
                var meta = new CacheMeta
                {
                    AbsoluteExpire = absoluteExpire,
                    SlidingExpire = options.SlidingExpire,
                };

                // 缓存到本地内存
                var cachePolicy = new MemoryCacheEntryOptions()
                {
                    AbsoluteExpiration = absoluteExpire,
                    SlidingExpiration = options.SlidingExpire,
                };
                MemoryCache.Set(cacheKey, new LocalCacheEntry<TData>(value, meta), cachePolicy);
            }
        }


        /// <inheritdoc />
        public virtual bool Refresh(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            var cacheKey = GetCacheKey(key);
            CacheMeta meta;

            if (StoragePolicy.HasFlag(CacheStoragePolicy.Memory) &&
                MemoryCache.Get(cacheKey) is LocalCacheEntry<TData> localCache)
            {
                meta = localCache.Meta;
            }
            else if (StoragePolicy.HasFlag(CacheStoragePolicy.Redis))
            {
                meta = GetMetaFromRedis(key);
            }
            else
            {
                return false;
            }

            if (StoragePolicy.HasFlag(CacheStoragePolicy.Redis))
            {
                CheckAndRefreshRedisSlidingExpire(meta, cacheKey);
            }

            return true;
        }

        /// <inheritdoc />
        public virtual bool Remove(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            var rs = false;

            var cacheKey = GetCacheKey(key);
            if (StoragePolicy.HasFlag(CacheStoragePolicy.Redis))
            {
                rs = RedisCache.KeyDelete(cacheKey);
                CacheStorage.PublishCacheExpired(cacheKey);
            }

            if (StoragePolicy.HasFlag(CacheStoragePolicy.Memory))
            {
                MemoryCache.Remove(cacheKey);
                rs = true;
            }
            return rs;
        }

        #endregion

        #region Private

        /// <summary>
        /// 获取缓存数据
        /// </summary>
        protected virtual async ValueTask<(bool Exists, TData Data)> InternalGetAsync(string key)
        {
            var cacheKey = GetCacheKey(key);

            if (StoragePolicy.HasFlag(CacheStoragePolicy.Memory))
            {
                // 本地内存缓存
                if (MemoryCache.TryGetValue<LocalCacheEntry<TData>>(cacheKey, out var localCache))
                {
                    if (StoragePolicy.HasFlag(CacheStoragePolicy.Redis))
                    {
                        await CheckAndRefreshRedisSlidingExpireAsync(localCache.Meta, cacheKey);
                    }

                    return (localCache.Data != null, localCache.Data);
                }
            }

            if (StoragePolicy.HasFlag(CacheStoragePolicy.Redis))
            {
                // Redis 缓存
                var entry = await GetFromRedisAsync(cacheKey);

                if (entry.Exists)
                {
                    var data = entry.Data;
                    var meta = await GetMetaFromRedisAsync(key);

                    // 缓存到本地内存
                    if (StoragePolicy.HasFlag(CacheStoragePolicy.Memory))
                    {
                        var cachePolicy = new MemoryCacheEntryOptions()
                        {
                            AbsoluteExpiration = meta.AbsoluteExpire,
                            SlidingExpiration = meta.SlidingExpire,
                        };
                        MemoryCache.Set(cacheKey, new LocalCacheEntry<TData>(data, meta), cachePolicy);
                    }

                    await CheckAndRefreshRedisSlidingExpireAsync(meta, cacheKey);
                    return (data != null, data);
                }
            }

            return (false, default);
        }

        /// <summary>
        /// 获取缓存数据
        /// </summary>
        protected virtual (bool Exists, TData Data) InternalGet(string key)
        {
            var cacheKey = GetCacheKey(key);

            if (StoragePolicy.HasFlag(CacheStoragePolicy.Memory))
            {
                // 本地内存缓存
                if (MemoryCache.TryGetValue<LocalCacheEntry<TData>>(cacheKey, out var localCache))
                {
                    if (StoragePolicy.HasFlag(CacheStoragePolicy.Redis))
                    {
                        CheckAndRefreshRedisSlidingExpire(localCache.Meta, cacheKey);
                    }

                    return (localCache.Data != null, localCache.Data);
                }
            }

            if (StoragePolicy.HasFlag(CacheStoragePolicy.Redis))
            {
                // Redis 缓存
                var entry = GetFromRedis(cacheKey);

                if (entry.Exists)
                {
                    var data = entry.Data;
                    var meta = GetMetaFromRedis(key);

                    // 缓存到本地内存
                    if (StoragePolicy.HasFlag(CacheStoragePolicy.Memory))
                    {
                        var cachePolicy = new MemoryCacheEntryOptions()
                        {
                            AbsoluteExpiration = meta.AbsoluteExpire,
                            SlidingExpiration = meta.SlidingExpire,
                        };
                        MemoryCache.Set(cacheKey, new LocalCacheEntry<TData>(data, meta), cachePolicy);
                    }

                    CheckAndRefreshRedisSlidingExpire(meta, cacheKey);
                    return (data != null, data);
                }
            }

            return (false, default);
        }

        /// <summary>
        /// 从Redis获取缓存数据
        /// </summary>
        protected virtual async Task<(bool Exists, TData Data)> GetFromRedisAsync(string cacheKey)
        {
            var redis = RedisCache;
            var redisValue = await redis.HashGetAsync(cacheKey, DataKey);

            if (redisValue.HasValue)
            {
                string json = redisValue;
                var data = json is TData str2 ? str2 : JsonConvert.DeserializeObject<TData>(json, Config.SerializerSettings);
                return (data != null, data);
            }

            return (false, default);
        }

        /// <summary>
        /// 从Redis获取缓存数据
        /// </summary>
        protected virtual (bool Exists, TData Data) GetFromRedis(string cacheKey)
        {
            var redis = RedisCache;
            var redisValue = redis.HashGet(cacheKey, DataKey);

            if (redisValue.HasValue)
            {
                string json = redisValue;
                var data = json is TData str2 ? str2 : JsonConvert.DeserializeObject<TData>(json, Config.SerializerSettings);
                return (data != null, data);
            }

            return (false, default);
        }

        /// <summary>
        /// 设置Redis缓存数据
        /// </summary>
        protected virtual async Task SetToRedisAsync(string key, TData value, CacheEntryOptions options)
        {
            var cacheKey = GetCacheKey(key);
            var data = value is string str ? str : JsonConvert.SerializeObject(value, Config.SerializerSettings);

            var creationTime = DateTimeOffset.UtcNow;
            var absoluteExpire = creationTime + options.AbsoluteExpire;
            var expire = GetExpire(creationTime, absoluteExpire, options.SlidingExpire);

            var tran = RedisCache.CreateTransaction();
            _ = tran.HashSetAsync(cacheKey, new[]
            {
                new HashEntry(DataKey, data),
                new HashEntry(AbsoluteExpireKey, absoluteExpire?.Ticks ?? NotPresent),
                new HashEntry(SlidingExpireKey, options.SlidingExpire?.Ticks ?? NotPresent)
            });
            _ = tran.KeyExpireAsync(cacheKey, expire);
            await tran.ExecuteAsync();
            await CacheStorage.PublishCacheExpiredAsync(cacheKey);
        }

        /// <summary>
        /// 设置Redis缓存数据
        /// </summary>
        protected virtual void SetToRedis(string key, TData value, CacheEntryOptions options)
        {
            var cacheKey = GetCacheKey(key);
            var data = value is string str ? str : JsonConvert.SerializeObject(value, Config.SerializerSettings);

            var creationTime = DateTimeOffset.UtcNow;
            var absoluteExpire = creationTime + options.AbsoluteExpire;
            var expire = GetExpire(creationTime, absoluteExpire, options.SlidingExpire);

            var tran = RedisCache.CreateTransaction();
            _ = tran.HashSetAsync(cacheKey, new[]
            {
                new HashEntry(DataKey, data),
                new HashEntry(AbsoluteExpireKey, absoluteExpire?.Ticks ?? NotPresent),
                new HashEntry(SlidingExpireKey, options.SlidingExpire?.Ticks ?? NotPresent)
            });
            _ = tran.KeyExpireAsync(cacheKey, expire);
            tran.Execute();
            CacheStorage.PublishCacheExpired(cacheKey);
        }

        /// <summary>
        /// 检查和更新滑动过期时间
        /// </summary>
        private async Task<bool> CheckAndRefreshRedisSlidingExpireAsync(CacheMeta meta, string cacheKey)
        {
            if (meta.SlidingExpire.HasValue && meta.TimeToLive.HasValue)
            {
                var now = DateTimeOffset.UtcNow;
                var ttl = meta.TimeToLive.Value - now;

                // 更新条件： 剩余时间 < ( 滑动过期时间 / 2 )
                if (ttl.Ticks < meta.SlidingExpire.Value.Ticks / 2)
                {
                    var expire = GetExpire(now, meta.AbsoluteExpire, meta.SlidingExpire);
                    await RedisCache.KeyExpireAsync(cacheKey, expire);
                    meta.TimeToLive = now + expire;

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 检查和更新滑动过期时间
        /// </summary>
        private bool CheckAndRefreshRedisSlidingExpire(CacheMeta meta, string cacheKey)
        {
            if (meta.SlidingExpire.HasValue && meta.TimeToLive.HasValue)
            {
                var now = DateTimeOffset.UtcNow;
                var ttl = meta.TimeToLive.Value - now;

                // 更新条件： 剩余时间 < ( 滑动过期时间 / 2 )
                if (ttl.Ticks < meta.SlidingExpire.Value.Ticks / 2)
                {
                    var expire = GetExpire(now, meta.AbsoluteExpire, meta.SlidingExpire);
                    RedisCache.KeyExpire(cacheKey, expire);
                    meta.TimeToLive = now + expire;

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 获取元数据
        /// </summary>
        protected virtual async Task<CacheMeta> GetMetaFromRedisAsync(string key)
        {
            var cacheKey = GetCacheKey(key);
            var metaFidles = new RedisValue[] { AbsoluteExpireKey, SlidingExpireKey };
            var redis = RedisCache;
            var results = await redis.HashGetAsync(cacheKey, metaFidles);
            var ttl = await redis.KeyTimeToLiveAsync(cacheKey);
            var meta = new CacheMeta { TimeToLive = DateTimeOffset.UtcNow + ttl };

            if (results.Length >= metaFidles.Length)
            {
                var expirationTicks = (long?)results[0];
                if (expirationTicks.HasValue && expirationTicks.Value != NotPresent)
                {
                    meta.AbsoluteExpire = new DateTimeOffset(expirationTicks.Value, TimeSpan.Zero);
                }

                var slidingTicks = (long?)results[1];
                if (slidingTicks.HasValue && slidingTicks.Value != NotPresent)
                {
                    meta.SlidingExpire = new TimeSpan(slidingTicks.Value);
                }
            }

            return meta;
        }

        /// <summary>
        /// 获取元数据
        /// </summary>
        protected virtual CacheMeta GetMetaFromRedis(string key)
        {
            var cacheKey = GetCacheKey(key);
            var metaFidles = new RedisValue[] { AbsoluteExpireKey, SlidingExpireKey };
            var redis = RedisCache;
            var results = redis.HashGet(cacheKey, metaFidles);
            var ttl = redis.KeyTimeToLive(cacheKey);
            var meta = new CacheMeta { TimeToLive = DateTimeOffset.UtcNow + ttl };

            if (results.Length >= metaFidles.Length)
            {
                var expirationTicks = (long?)results[0];
                if (expirationTicks.HasValue && expirationTicks.Value != NotPresent)
                {
                    meta.AbsoluteExpire = new DateTimeOffset(expirationTicks.Value, TimeSpan.Zero);
                }

                var slidingTicks = (long?)results[1];
                if (slidingTicks.HasValue && slidingTicks.Value != NotPresent)
                {
                    meta.SlidingExpire = new TimeSpan(slidingTicks.Value);
                }
            }

            return meta;
        }

        /// <summary>
        /// 获取过期时间
        /// </summary>
        protected static TimeSpan? GetExpire(DateTimeOffset creationTime, DateTimeOffset? absoluteExpire, TimeSpan? slidingExpire)
        {
            if (absoluteExpire.HasValue && slidingExpire.HasValue)
            {
                var abs = absoluteExpire.Value - creationTime;
                var sld = slidingExpire.Value;

                return abs < sld ? abs : sld;
            }

            if (absoluteExpire.HasValue)
            {
                return absoluteExpire.Value - creationTime;
            }

            return slidingExpire;
        }

        #endregion
    }
}

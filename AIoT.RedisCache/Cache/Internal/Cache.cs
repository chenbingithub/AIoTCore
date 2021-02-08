using System;
using System.Threading.Tasks;
using AIoT.Core.Cache;
using Microsoft.Extensions.Caching.Memory;
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
        public Cache(IMemoryCache memoryCache, IConnectionMultiplexer redis, CacheOptions config, string cacheName = null)
            : base(memoryCache, redis, config, cacheName)
        {
        }

        #endregion


        #region IDistributedCache<TCacheItem>

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

            var rs = await InternalGetAsync(key);
            if (rs.Exists)
            {
                return rs.Data;
            }

            var value = await factory();
            if (value != null)
            {
                await SetAsync(key, value, optionsFactory?.Invoke());
            }

            return value;
        }

        /// <inheritdoc />
        public virtual async Task SetAsync(string key, TData value, CacheEntryOptions options = null)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (options == null) options = Options;

            await SetToRedisAsync(key, value, options);
            MemoryCache.Remove(GetCacheKey(key));
        }


        /// <inheritdoc />
        public virtual async Task<bool> RefreshAsync(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            var cacheKey = GetCacheKey(key);
            CacheMeta meta;
            if (MemoryCache.Get(cacheKey) is LocalCacheEntry<TData> localCache)
            {
                meta = localCache.Meta;
            }
            else
            {
                meta = await GetMetaAsync(key);
            }

            return await CheckAndRefreshSlidingExpireAsync(meta, cacheKey);
        }

        /// <inheritdoc />
        public virtual async Task<bool> RemoveAsync(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            var cacheKey = GetCacheKey(key);

            var tran = RedisCache.CreateTransaction();
            _ = tran.KeyDeleteAsync(cacheKey);
            _ = tran.PublishAsync(CacheExpiredChannel, cacheKey);

            var rs = await tran.ExecuteAsync();
            MemoryCache.Remove(cacheKey);
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

            // 本地内存缓存
            if (MemoryCache.TryGetValue<LocalCacheEntry<TData>>(cacheKey, out var localCache))
            {
                await CheckAndRefreshSlidingExpireAsync(localCache.Meta, cacheKey);
                return (true, localCache.Data);
            }

            // Redis 缓存
            var batch = RedisCache.CreateBatch();
            var taskData = GetFromRedisAsync(cacheKey, batch);
            var taskMeta = GetMetaAsync(key, batch);
            batch.Execute();
            await Task.WhenAll(taskData, taskMeta);

            if (taskData.Result.Exists)
            {
                var data = taskData.Result.Data;
                var meta = taskMeta.Result;

                // 缓存到本地内存
                var cachePolicy = new MemoryCacheEntryOptions()
                {
                    AbsoluteExpiration = meta.AbsoluteExpire,
                    SlidingExpiration = meta.SlidingExpire,
                };
                MemoryCache.Set(cacheKey, new LocalCacheEntry<TData>(data, meta), cachePolicy);

                await CheckAndRefreshSlidingExpireAsync(meta, cacheKey);
                return (true, data);
            }

            return (false, default);
        }

        /// <summary>
        /// 从Redis获取缓存数据
        /// </summary>
        protected virtual async Task<(bool Exists, TData Data)> GetFromRedisAsync(string cacheKey, IDatabaseAsync batch = null)
        {
            var redis = batch ?? RedisCache;
            var redisValue = await redis.HashGetAsync(cacheKey, DataKey);

            if (redisValue.HasValue)
            {
                string json = redisValue;
                var data = json is TData str2 ? str2 : JsonConvert.DeserializeObject<TData>(json);
                return (true, data);
            }

            return (false, default);
        }

        /// <summary>
        /// 设置Redis缓存数据
        /// </summary>
        protected virtual async Task SetToRedisAsync(string key, TData value, CacheEntryOptions options)
        {
            var cacheKey = GetCacheKey(key);
            var data = value is string str ? str : JsonConvert.SerializeObject(value);

            var creationTime = DateTimeOffset.UtcNow;
            var absoluteExpire = GetAbsoluteExpire(creationTime, options.AbsoluteExpire);
            var expire = GetExpire(creationTime, absoluteExpire, options.SlidingExpire);

            var tran = RedisCache.CreateTransaction();
            _ = tran.HashSetAsync(cacheKey, new[]
            {
                new HashEntry(DataKey, data),
                new HashEntry(AbsoluteExpireKey, absoluteExpire?.Ticks ?? NotPresent),
                new HashEntry(SlidingExpireKey, options.SlidingExpire?.Ticks ?? NotPresent)
            });
            _ = tran.KeyExpireAsync(cacheKey, expire);
            _ = tran.PublishAsync(CacheExpiredChannel, cacheKey);

            await tran.ExecuteAsync();
        }

        /// <summary>
        /// 检查和更新滑动过期时间
        /// </summary>
        private async Task<bool> CheckAndRefreshSlidingExpireAsync(CacheMeta meta, string cacheKey)
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
        /// 获取元数据
        /// </summary>
        protected virtual async Task<CacheMeta> GetMetaAsync(string key, IDatabaseAsync batch = null)
        {
            var cacheKey = GetCacheKey(key);
            var metaFidles = new RedisValue[] { AbsoluteExpireKey, SlidingExpireKey };
            var redis = batch ?? RedisCache;
            var t1 = redis.HashGetAsync(cacheKey, metaFidles);
            var t2 = redis.KeyTimeToLiveAsync(cacheKey);
            await Task.WhenAll(t1, t2);

            var results = t1.Result;
            var ttl = t2.Result;
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

        /// <summary>
        /// 获取绝对过期时间
        /// </summary>
        protected static DateTimeOffset? GetAbsoluteExpire(DateTimeOffset creationTime, TimeSpan? absoluteExpire)
        {
            return creationTime + absoluteExpire;
        }

        #endregion
    }
}

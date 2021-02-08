using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AIoT.Core.Cache;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace AIoT.RedisCache.Cache.Internal
{
    /// <summary>
    /// 分布式集合缓存实现
    /// </summary>
    public class ListCache<TData, TId> : CacheBase, IListCache<TData, TId>
    {
        private static readonly TypeCode _typeCode = Type.GetTypeCode(typeof(TId));

        /// <summary>
        /// <inheritdoc cref="ListCache{TData, TId}"/>
        /// </summary>
        public ListCache(IMemoryCache memoryCache, IConnectionMultiplexer redis, CacheOptions config, string cacheName = null) 
            : base(memoryCache, redis, config, cacheName)
        {
        }

        /// <inheritdoc />
        public async ValueTask<TData> GetAsync(string key, TId id)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (id == null) throw new ArgumentNullException(nameof(id));

            var (exists, data) = await InternalGetAsync(key);
            return exists && data.TryGetValue(id, out var value) ? value : default;
        }

        /// <inheritdoc />
        public async ValueTask<IEnumerable<TData>> GetAllAsync(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            var (exists, data) = await InternalGetAsync(key);
            return exists ? data.Values : default;
        }

        /// <inheritdoc />
        public async ValueTask<IEnumerable<TData>> GetAllAsync(string key, IEnumerable<TId> ids)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (ids == null || !ids.Any()) return new TData[0];

            var (exists, data) = await InternalGetAsync(key);
            if (!exists) return default;

            var list = new List<TData>(ids.Count());
            foreach (var id in ids)
            {
                list.Add(data.TryGetValue(id, out var value) ? value : default);
            }

            return list;
        }

        /// <inheritdoc />
        public async ValueTask<IReadOnlyDictionary<TId, TData>> GetAsync(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            var (exists, data) = await InternalGetAsync(key);
            return exists ? data : default;
        }

        /// <inheritdoc />
        public async ValueTask<IReadOnlyDictionary<TId, TData>> GetOrAddAsync(string key, Func<Task<IDictionary<TId, TData>>> factory, Func<CacheEntryOptions> optionsFactory = null)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            var (exists, data) = await InternalGetAsync(key);
            if (exists)
            {
                return data;
            }

            var value = await factory();
            if (value != null)
            {
                await SetAsync(key, value, optionsFactory?.Invoke());
                return new ReadOnlyDictionary<TId, TData>(value);
            }

            return null;
        }

        /// <inheritdoc />
        public async ValueTask<TData> GetOrAddAsync(string key, TId id, Func<TId, Task<TData>> factory, Func<CacheEntryOptions> optionsFactory = null)
        {
            var (exists, data) = await InternalGetAsync(key);
            if (exists && data.TryGetValue(id, out var value))
            {
                return value;
            }

            value = await factory(id);
            if (value != null)
            {
                await SetAsync(key, id, value, optionsFactory?.Invoke());
            }

            return value;
        }

        /// <inheritdoc />
        public async Task SetAsync(string key, IDictionary<TId, TData> values, CacheEntryOptions options = null)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (values == null) throw new ArgumentNullException(nameof(values));
            if (options == null) options = Options;

            await SetToRedisAsync(key, values, options, true);
            MemoryCache.Remove(GetCacheKey(key));
        }

        /// <inheritdoc />
        public async Task SetAsync(string key, TId id, TData value, CacheEntryOptions options = null)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (options == null) options = Options;

            var values = new Dictionary<TId, TData>() { { id, value } };
            await SetToRedisAsync(key, values, options, false);
            MemoryCache.Remove(GetCacheKey(key));
        }

        /// <inheritdoc />
        public async Task<bool> RefreshAsync(string key)
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
        public async Task<bool> RemoveAsync(string key)
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

        /// <inheritdoc />
        public async Task<bool> RemoveAsync(string key, TId id)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            var cacheKey = GetCacheKey(key);

            var tran = RedisCache.CreateTransaction();
            _ = tran.HashDeleteAsync(cacheKey, ConvertClrToRedisValue(id));
            _ = tran.PublishAsync(CacheExpiredChannel, cacheKey);
            var rs = await tran.ExecuteAsync();

            MemoryCache.Remove(cacheKey);
            return rs;
        }

        /// <summary>
        /// 获取缓存数据
        /// </summary>
        protected virtual async ValueTask<(bool Exists, Dictionary<TId, TData> Data)> InternalGetAsync(string key)
        {
            var cacheKey = GetCacheKey(key);

            // 本地内存缓存
            if (MemoryCache.TryGetValue<LocalCacheEntry<Dictionary<TId, TData>>>(cacheKey, out var localCache))
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
                    AbsoluteExpiration = meta.AbsoluteExpire ,
                    SlidingExpiration = meta.SlidingExpire,
                };
                MemoryCache.Set(cacheKey, new LocalCacheEntry<Dictionary<TId, TData>>(data, meta), cachePolicy);

                await CheckAndRefreshSlidingExpireAsync(meta, cacheKey);
                return (true, data);
            }

            return (false, default);
        }

        /// <summary>
        /// 从Redis获取缓存数据
        /// </summary>
        protected virtual async Task<(bool Exists, Dictionary<TId, TData> Data)> GetFromRedisAsync(
            string cacheKey, IDatabaseAsync batch = null)
        {
            var redis = batch ?? RedisCache;
            var entries = await redis.HashGetAllAsync(cacheKey);

            if (entries.Length == 0) return (false, default);

            var dictionary = new Dictionary<TId, TData>();
            foreach (var entry in entries)
            {
                if (entry.Name == AbsoluteExpireKey || entry.Name == SlidingExpireKey) continue;

                var field = ConvertRedisToClrValue(entry.Name);
                string json = entry.Value;
                var data = json is TData str2 ? str2 : JsonConvert.DeserializeObject<TData>(json);
                dictionary.Add(field, data);
            }

            return (true, dictionary);
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
        /// 保存到Redis
        /// </summary>
        private async Task SetToRedisAsync(string key, IDictionary<TId, TData> values, CacheEntryOptions options, bool removeOld)
        {
            var cacheKey = GetCacheKey(key);

            var creationTime = DateTimeOffset.UtcNow;
            var absoluteExpire = creationTime + options.AbsoluteExpire;
            var expire = GetExpire(creationTime, absoluteExpire, options.SlidingExpire);

            // Data
            var i = 0;
            var entries = new HashEntry[values.Count + 2];
            foreach (var item in values)
            {
                var field = ConvertClrToRedisValue(item.Key);
                var data = item.Value is string str ? str : JsonConvert.SerializeObject(item.Value);
                entries[i++] = new HashEntry(field, data);
            }

            // Meta
            entries[i++] = new HashEntry(AbsoluteExpireKey, absoluteExpire?.Ticks ?? NotPresent);
            entries[i] = new HashEntry(SlidingExpireKey, options.SlidingExpire?.Ticks ?? NotPresent);

            var tran = RedisCache.CreateTransaction();
            if (removeOld)
            {
                _ = tran.KeyDeleteAsync(cacheKey);
            }
            _ = tran.HashSetAsync(cacheKey, entries);
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


        private RedisValue ConvertClrToRedisValue(TId val)
        {
            switch (val)
            {
                case bool boolVal:
                    return boolVal;
                case char charVal:
                    return (int)charVal;
                case sbyte sbyteVal:
                    return sbyteVal;
                case byte byteVal:
                    return (int)byteVal;
                case short shortVal:
                    return shortVal;
                case ushort ushortVal:
                    return (int)ushortVal;
                case int intVal:
                    return intVal;
                case uint uintVal:
                    return uintVal;
                case long longVal:
                    return longVal;
                case ulong ulongVal:
                    return ulongVal;
                case float floatVal:
                    return floatVal;
                case double doubleVal:
                    return doubleVal;
                case decimal decimalVal:
                    return (double)decimalVal;
                case string strVal:
                    return strVal;
                case Guid guidVal:
                    return guidVal.ToString();
                default:
                    throw new NotSupportedException();
            }
        }

        private TId ConvertRedisToClrValue(RedisValue val)
        {
            if (typeof(TId) == typeof(Guid))
            {
                return (TId)(object)new Guid((string)val);
            }

            return (TId)Convert.ChangeType(val, _typeCode);
        }
    }
}

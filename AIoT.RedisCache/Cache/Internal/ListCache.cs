using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
    /// 分布式集合缓存实现
    /// </summary>
    public class ListCache<TData, TId> : CacheBase, IListCache<TData, TId>
    {
        #region 构造函数、私有字段

        private static readonly TypeCode _typeCode = Type.GetTypeCode(typeof(TId));

        /// <summary>
        /// <inheritdoc cref="ListCache{TData, TId}"/>
        /// </summary>
        public ListCache(ICacheStorage cacheStorage, IOptions<CacheOptions> config)
            : this(cacheStorage, config, CacheOptions.DefaultCacheName)
        {
        }

        /// <summary>
        /// <inheritdoc cref="ListCache{TData, TId}"/>
        /// </summary>
        public ListCache(ICacheStorage cacheStorage, IOptions<CacheOptions> config, string cacheName)
            : base(cacheStorage, config, cacheName)
        {
        }


        #endregion

        #region IListCache<TData, TId>

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

            return ids.Select(id => data.TryGetValue(id, out var value) ? value : default).ToList();
        }

        /// <inheritdoc />
        public async ValueTask<IEnumerable<TId>> GetAllIdsAsync(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            var (exists, data) = await InternalGetAsync(key);
            return exists ? data.Keys : default;
        }

        /// <inheritdoc />
        public async ValueTask<IReadOnlyDictionary<TId, TData>> GetAsync(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            var (exists, data) = await InternalGetAsync(key);
            return exists ? data : default;
        }

        /// <inheritdoc />
        public async ValueTask<IReadOnlyDictionary<TId, TData>> GetOrAddAsync(string key, Func<Task<IReadOnlyDictionary<TId, TData>>> factory, Func<CacheEntryOptions> optionsFactory = null)
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
                        return value;
                    }

                    return null;
                }
            }
            finally
            {
                Locks.TryRemove(key, out _);
            }
        }

        /// <inheritdoc />
        public async ValueTask<TData> GetOrAddAsync(string key, TId id, Func<TId, Task<TData>> factory, Func<CacheEntryOptions> optionsFactory = null)
        {
            var (exists, data) = await InternalGetAsync(key);
            if (exists && data.TryGetValue(id, out var value))
            {
                return value;
            }

            try
            {
                using (await Locks.GetOrAdd(key, p => new AsyncLock()).LockAsync())
                {
                    (exists, data) = await InternalGetAsync(key);
                    if (exists && data.TryGetValue(id, out value))
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
            }
            finally
            {
                Locks.TryRemove(key, out _);
            }
        }

        /// <inheritdoc />
        public async Task SetAsync(string key, IReadOnlyDictionary<TId, TData> values, CacheEntryOptions options = null)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (values == null) throw new ArgumentNullException(nameof(values));
            if (options == null) options = Options;

            if (StoragePolicy.HasFlag(CacheStoragePolicy.Redis))
            {
                // 缓存到Redis
                await SetToRedisAsync(key, values, options, true);
                if (StoragePolicy.HasFlag(CacheStoragePolicy.Memory))
                {
                    MemoryCache.Remove(GetCacheKey(key));
                }
            }
            else if (StoragePolicy.HasFlag(CacheStoragePolicy.Memory))
            {
                // 缓存到本地内存
                var absoluteExpire = DateTimeOffset.UtcNow + options.AbsoluteExpire;
                var meta = new CacheMeta
                {
                    AbsoluteExpire = absoluteExpire,
                    SlidingExpire = options.SlidingExpire,
                };

                var cachePolicy = new MemoryCacheEntryOptions()
                {
                    AbsoluteExpiration = absoluteExpire,
                    SlidingExpiration = options.SlidingExpire,
                };
                var data = new Dictionary<TId, TData>(values);
                MemoryCache.Set(GetCacheKey(key), new LocalCacheEntry<TId, TData>(data, meta), cachePolicy);
            }
        }

        /// <inheritdoc />
        public async Task SetAsync(string key, TId id, TData value, CacheEntryOptions options = null)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (options == null) options = Options;

            if (StoragePolicy.HasFlag(CacheStoragePolicy.Redis))
            {
                var values = new Dictionary<TId, TData>() { { id, value } };
                await SetToRedisAsync(key, values, options, false);

                if (StoragePolicy.HasFlag(CacheStoragePolicy.Memory))
                {
                    MemoryCache.Remove(GetCacheKey(key));
                }
            }
            else if (StoragePolicy.HasFlag(CacheStoragePolicy.Memory))
            {
                var cacheKey = GetCacheKey(key);
                var data = MemoryCache.TryGetValue<LocalCacheEntry<TId, TData>>(cacheKey, out var localCache)
                    ? new Dictionary<TId, TData>(localCache.Data) : new Dictionary<TId, TData>();

                // 增加缓存项
                data[id] = value;
                var absoluteExpire = DateTimeOffset.UtcNow + options.AbsoluteExpire;
                var meta = new CacheMeta
                {
                    AbsoluteExpire = absoluteExpire,
                    SlidingExpire = options.SlidingExpire,
                };

                var cachePolicy = new MemoryCacheEntryOptions()
                {
                    AbsoluteExpiration = absoluteExpire,
                    SlidingExpiration = options.SlidingExpire,
                };
                MemoryCache.Set(GetCacheKey(key), new LocalCacheEntry<TId, TData>(data, meta), cachePolicy);
            }
        }

        /// <inheritdoc />
        public async Task<bool> RefreshAsync(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            var cacheKey = GetCacheKey(key);
            CacheMeta meta;

            if (StoragePolicy.HasFlag(CacheStoragePolicy.Memory) &&
                MemoryCache.Get(cacheKey) is LocalCacheEntry<TId, TData> localCache)
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
        public async Task<bool> RemoveAsync(string key)
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

        /// <inheritdoc />
        public async Task<bool> RemoveAsync(string key, TId id)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            var rs = false;

            var cacheKey = GetCacheKey(key);
            if (StoragePolicy.HasFlag(CacheStoragePolicy.Redis))
            {
                rs = await RedisCache.HashDeleteAsync(cacheKey, ConvertClrToRedisValue(id));
                await CacheStorage.PublishCacheExpiredAsync(cacheKey);
            }

            if (StoragePolicy.HasFlag(CacheStoragePolicy.Memory))
            {
                if (MemoryCache.TryGetValue<LocalCacheEntry<TId, TData>>(cacheKey, out var localCache))
                {
                    // 移除缓存项
                    var meta = localCache.Meta;
                    if (localCache.Data.ContainsKey(id))
                    {
                        var data = new Dictionary<TId, TData>(localCache.Data);
                        data.Remove(id);
                        var cachePolicy = new MemoryCacheEntryOptions()
                        {
                            AbsoluteExpiration = meta.AbsoluteExpire,
                            SlidingExpiration = meta.SlidingExpire,
                        };

                        var cacheData = new LocalCacheEntry<TId, TData>(data, meta);
                        MemoryCache.Set(GetCacheKey(key), cacheData, cachePolicy);
                        rs = true;
                    }
                }
            }

            return rs;
        }

        #endregion

        #region ISyncListCache<TData, TId>

        /// <inheritdoc />
        public TData Get(string key, TId id)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (id == null) throw new ArgumentNullException(nameof(id));

            var (exists, data) = InternalGet(key);
            return exists && data.TryGetValue(id, out var value) ? value : default;
        }

        /// <inheritdoc />
        public IEnumerable<TData> GetAll(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            var (exists, data) = InternalGet(key);
            return exists ? data.Values : default;
        }

        /// <inheritdoc />
        public IEnumerable<TData> GetAll(string key, IEnumerable<TId> ids)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (ids == null || !ids.Any()) return new TData[0];

            var (exists, data) = InternalGet(key);
            if (!exists) return default;

            return ids.Select(id => data.TryGetValue(id, out var value) ? value : default).ToList();
        }

        /// <inheritdoc />
        public IEnumerable<TId> GetAllIds(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            var (exists, data) = InternalGet(key);
            return exists ? data.Keys : default;
        }

        /// <inheritdoc />
        public IReadOnlyDictionary<TId, TData> Get(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            var (exists, data) = InternalGet(key);
            return exists ? data : default;
        }

        /// <inheritdoc />
        public IReadOnlyDictionary<TId, TData> GetOrAdd(string key,
            Func<IReadOnlyDictionary<TId, TData>> factory, Func<CacheEntryOptions> optionsFactory = null)
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
                        return value;
                    }

                    return null;
                }
            }
            finally
            {
                Locks.TryRemove(key, out _);
            }
        }

        /// <inheritdoc />
        public TData GetOrAdd(string key, TId id, Func<TId, TData> factory,
            Func<CacheEntryOptions> optionsFactory = null)
        {
            var (exists, data) = InternalGet(key);
            if (exists && data.TryGetValue(id, out var value))
            {
                return value;
            }

            try
            {
                using (Locks.GetOrAdd(key, p => new AsyncLock()).Lock())
                {
                    (exists, data) = InternalGet(key);
                    if (exists && data.TryGetValue(id, out value))
                    {
                        return value;
                    }

                    value = factory(id);
                    if (value != null)
                    {
                        Set(key, id, value, optionsFactory?.Invoke());
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
        public void Set(string key, IReadOnlyDictionary<TId, TData> values, CacheEntryOptions options = null)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (values == null) throw new ArgumentNullException(nameof(values));
            if (options == null) options = Options;

            if (StoragePolicy.HasFlag(CacheStoragePolicy.Redis))
            {
                // 缓存到Redis
                SetToRedis(key, values, options, true);
                if (StoragePolicy.HasFlag(CacheStoragePolicy.Memory))
                {
                    MemoryCache.Remove(GetCacheKey(key));
                }
            }
            else if (StoragePolicy.HasFlag(CacheStoragePolicy.Memory))
            {
                // 缓存到本地内存
                var absoluteExpire = DateTimeOffset.UtcNow + options.AbsoluteExpire;
                var meta = new CacheMeta
                {
                    AbsoluteExpire = absoluteExpire,
                    SlidingExpire = options.SlidingExpire,
                };

                var cachePolicy = new MemoryCacheEntryOptions()
                {
                    AbsoluteExpiration = absoluteExpire,
                    SlidingExpiration = options.SlidingExpire,
                };
                var data = new Dictionary<TId, TData>(values);
                MemoryCache.Set(GetCacheKey(key), new LocalCacheEntry<TId, TData>(data, meta), cachePolicy);
            }
        }

        /// <inheritdoc />
        public void Set(string key, TId id, TData value, CacheEntryOptions options = null)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (options == null) options = Options;

            if (StoragePolicy.HasFlag(CacheStoragePolicy.Redis))
            {
                var values = new Dictionary<TId, TData>() { { id, value } };
                SetToRedis(key, values, options, false);

                if (StoragePolicy.HasFlag(CacheStoragePolicy.Memory))
                {
                    MemoryCache.Remove(GetCacheKey(key));
                }
            }
            else if (StoragePolicy.HasFlag(CacheStoragePolicy.Memory))
            {
                var cacheKey = GetCacheKey(key);
                var data = MemoryCache.TryGetValue<LocalCacheEntry<TId, TData>>(cacheKey, out var localCache)
                    ? new Dictionary<TId, TData>(localCache.Data) : new Dictionary<TId, TData>();

                // 增加缓存项
                data[id] = value;
                var absoluteExpire = DateTimeOffset.UtcNow + options.AbsoluteExpire;
                var meta = new CacheMeta
                {
                    AbsoluteExpire = absoluteExpire,
                    SlidingExpire = options.SlidingExpire,
                };

                var cachePolicy = new MemoryCacheEntryOptions()
                {
                    AbsoluteExpiration = absoluteExpire,
                    SlidingExpiration = options.SlidingExpire,
                };
                MemoryCache.Set(GetCacheKey(key), new LocalCacheEntry<TId, TData>(data, meta), cachePolicy);
            }
        }

        /// <inheritdoc />
        public bool Refresh(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            var cacheKey = GetCacheKey(key);
            CacheMeta meta;

            if (StoragePolicy.HasFlag(CacheStoragePolicy.Memory) &&
                MemoryCache.Get(cacheKey) is LocalCacheEntry<TId, TData> localCache)
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
        public bool Remove(string key)
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

        /// <inheritdoc />
        public bool Remove(string key, TId id)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            var rs = false;

            var cacheKey = GetCacheKey(key);
            if (StoragePolicy.HasFlag(CacheStoragePolicy.Redis))
            {
                rs = RedisCache.HashDelete(cacheKey, ConvertClrToRedisValue(id));
                CacheStorage.PublishCacheExpired(cacheKey);
            }

            if (StoragePolicy.HasFlag(CacheStoragePolicy.Memory))
            {
                if (MemoryCache.TryGetValue<LocalCacheEntry<TId, TData>>(cacheKey, out var localCache))
                {
                    // 移除缓存项
                    var meta = localCache.Meta;
                    if (localCache.Data.ContainsKey(id))
                    {
                        var data = new Dictionary<TId, TData>(localCache.Data);
                        data.Remove(id);
                        var cachePolicy = new MemoryCacheEntryOptions()
                        {
                            AbsoluteExpiration = meta.AbsoluteExpire,
                            SlidingExpiration = meta.SlidingExpire,
                        };

                        var cacheData = new LocalCacheEntry<TId, TData>(data, meta);
                        MemoryCache.Set(GetCacheKey(key), cacheData, cachePolicy);
                        rs = true;
                    }
                }
            }

            return rs;
        }

        #endregion

        #region Private

        /// <summary>
        /// 获取缓存数据
        /// </summary>
        protected virtual async ValueTask<(bool Exists, IReadOnlyDictionary<TId, TData> Data)> InternalGetAsync(string key)
        {
            var cacheKey = GetCacheKey(key);

            if (StoragePolicy.HasFlag(CacheStoragePolicy.Memory))
            {
                // 本地内存缓存
                if (MemoryCache.TryGetValue<LocalCacheEntry<TId, TData>>(cacheKey, out var localCache))
                {
                    await CheckAndRefreshRedisSlidingExpireAsync(localCache.Meta, cacheKey);
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
                        MemoryCache.Set(cacheKey, new LocalCacheEntry<TId, TData>(data, meta), cachePolicy);
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
        protected virtual (bool Exists, IReadOnlyDictionary<TId, TData> Data) InternalGet(string key)
        {
            var cacheKey = GetCacheKey(key);

            if (StoragePolicy.HasFlag(CacheStoragePolicy.Memory))
            {
                // 本地内存缓存
                if (MemoryCache.TryGetValue<LocalCacheEntry<TId, TData>>(cacheKey, out var localCache))
                {
                    CheckAndRefreshRedisSlidingExpire(localCache.Meta, cacheKey);
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
                        MemoryCache.Set(cacheKey, new LocalCacheEntry<TId, TData>(data, meta), cachePolicy);
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
        protected virtual async Task<(bool Exists, IReadOnlyDictionary<TId, TData> Data)> GetFromRedisAsync(string cacheKey)
        {
            var redis = RedisCache;
            var entries = await redis.HashGetAllAsync(cacheKey);

            if (entries.Length == 0) return (false, default);

            var dictionary = new Dictionary<TId, TData>();
            foreach (var entry in entries)
            {
                if (entry.Name == AbsoluteExpireKey || entry.Name == SlidingExpireKey) continue;

                var field = ConvertRedisToClrValue(entry.Name);
                string json = entry.Value;
                var data = json is TData str2 ? str2 : JsonConvert.DeserializeObject<TData>(json, Config.SerializerSettings);
                dictionary.Add(field, data);
            }

            return (true, dictionary);
        }

        /// <summary>
        /// 从Redis获取缓存数据
        /// </summary>
        protected virtual (bool Exists, IReadOnlyDictionary<TId, TData> Data) GetFromRedis(string cacheKey)
        {
            var redis = RedisCache;
            var entries = redis.HashGetAll(cacheKey);

            if (entries.Length == 0) return (false, default);

            var dictionary = new Dictionary<TId, TData>();
            foreach (var entry in entries)
            {
                if (entry.Name == AbsoluteExpireKey || entry.Name == SlidingExpireKey) continue;

                var field = ConvertRedisToClrValue(entry.Name);
                string json = entry.Value;
                var data = json is TData str2 ? str2 : JsonConvert.DeserializeObject<TData>(json, Config.SerializerSettings);
                dictionary.Add(field, data);
            }

            return (true, dictionary);
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
        /// 保存到Redis
        /// </summary>
        private async Task SetToRedisAsync(string key, IReadOnlyDictionary<TId, TData> values, CacheEntryOptions options, bool removeOld)
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
                var data = item.Value is string str ? str : JsonConvert.SerializeObject(item.Value, Config.SerializerSettings);
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
            await tran.ExecuteAsync();
            await CacheStorage.PublishCacheExpiredAsync(cacheKey);
        }

        /// <summary>
        /// 保存到Redis
        /// </summary>
        private void SetToRedis(string key, IReadOnlyDictionary<TId, TData> values, CacheEntryOptions options, bool removeOld)
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
                var data = item.Value is string str ? str : JsonConvert.SerializeObject(item.Value, Config.SerializerSettings);
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
            return val switch
            {
                bool boolVal => boolVal,
                char charVal => (int)charVal,
                sbyte sbyteVal => sbyteVal,
                byte byteVal => (int)byteVal,
                short shortVal => shortVal,
                ushort ushortVal => (int)ushortVal,
                int intVal => intVal,
                uint uintVal => uintVal,
                long longVal => longVal,
                ulong ulongVal => ulongVal,
                float floatVal => floatVal,
                double doubleVal => doubleVal,
                decimal decimalVal => (double)decimalVal,
                string strVal => strVal,
                Guid guidVal => guidVal.ToString(),
                Enum enumVal => enumVal.ToString("D"),
                _ => val.ToString()
            };
        }

        private TId ConvertRedisToClrValue(RedisValue val)
        {
            if (typeof(TId) == typeof(Guid))
            {
                return (TId)(object)new Guid((string)val);
            }

            return (TId)Convert.ChangeType(val, _typeCode);
        }

        #endregion
    }
}

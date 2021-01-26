using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

//using StackExchange.Profiling;

namespace AIoT.RedisCache.Cache.Distributed
{
    /// <summary>
    /// 重新实现 Microsoft.Extensions.Caching.Distributed.RedisCache
    /// 原因：微软实现的 RedisCache 使用脚本方式（<see cref="IDatabase.ScriptEvaluate(string, RedisKey[], RedisValue[], CommandFlags)"/>）执行数据读写
    ///      腾讯云服务提供的 Redis 不支持脚本方式的命令，所以重新以命令方式实现 <see cref="IDistributedCache"/>
    /// </summary>
    public class RedisDistributedCache : IDistributedCache
    {
        private const string AbsoluteExpirationKey = "absexp";
        private const string SlidingExpirationKey = "sldexp";
        private const string DataKey = "data";
        private const long NotPresent = -1;

        private readonly IDatabase _cache;
        private readonly string _cachePrefix;

        /// <inheritdoc />
        public RedisDistributedCache(IOptions<RedisDistributedCacheOptions> optionsAccessor)
        {
            if (optionsAccessor == null)
            {
                throw new ArgumentNullException(nameof(optionsAccessor));
            }

            var options = optionsAccessor.Value;
            var connection = options.Connection;
            _cache = connection.GetDatabase(options.Db);

            _cachePrefix = options.Prefix ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(_cachePrefix))
            {
                _cachePrefix = _cachePrefix.EnsureEndsWith(':');
            }
        }

        /// <inheritdoc />
        public byte[] Get(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            //using (MiniProfiler.Current.CustomTiming(nameof(RedisDistributedCache), $"HMGET \"{GetCacheKey(key)}\"", "Get"))
            {
                return GetAndRefresh(key, getData: true);
            }
        }

        /// <inheritdoc />
        public async Task<byte[]> GetAsync(string key, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();

            //using (MiniProfiler.Current.CustomTiming($"{nameof(RedisDistributedCache)}(Async)", $"HMGET \"{GetCacheKey(key)}\"", "Get"))
            {
                return await GetAndRefreshAsync(key, getData: true, token: token);
            }
        }

        /// <inheritdoc />
        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var creationTime = DateTimeOffset.UtcNow;
            var absoluteExpiration = GetAbsoluteExpiration(creationTime, options);


            //using (MiniProfiler.Current.CustomTiming(nameof(RedisDistributedCache), $"HMSET \"{GetCacheKey(key)}\"", "Set"))
            {
                _cache.HashSet(GetCacheKey(key), new[]
                {
                    new HashEntry(AbsoluteExpirationKey, absoluteExpiration?.Ticks ?? NotPresent),
                    new HashEntry(SlidingExpirationKey, options.SlidingExpiration?.Ticks ?? NotPresent),
                    new HashEntry(DataKey, value)
                });

                var expiration = GetExpiration(creationTime, absoluteExpiration, options);
                if (expiration.HasValue)
                {
                    _cache.KeyExpire(GetCacheKey(key), expiration);
                }
            }
        }

        /// <inheritdoc />
        public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            token.ThrowIfCancellationRequested();


            var creationTime = DateTimeOffset.UtcNow;
            var absoluteExpiration = GetAbsoluteExpiration(creationTime, options);

            //using (MiniProfiler.Current.CustomTiming($"{nameof(RedisDistributedCache)}(Async)", $"HMSET \"{GetCacheKey(key)}\"", "Set"))
            {
                await _cache.HashSetAsync(GetCacheKey(key), new[]
                {
                    new HashEntry(AbsoluteExpirationKey, absoluteExpiration?.Ticks ?? NotPresent),
                    new HashEntry(SlidingExpirationKey, options.SlidingExpiration?.Ticks ?? NotPresent),
                    new HashEntry(DataKey, value)
                });

                var expiration = GetExpiration(creationTime, absoluteExpiration, options);
                if (expiration.HasValue)
                {
                    await _cache.KeyExpireAsync(GetCacheKey(key), expiration);
                }
            }
        }

        /// <inheritdoc />
        public void Refresh(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            //using (MiniProfiler.Current.CustomTiming(nameof(RedisDistributedCache), $"EXPIRE \"{GetCacheKey(key)}\"", "Refresh"))
            {
                GetAndRefresh(key, getData: false);
            }
        }

        /// <inheritdoc />
        public async Task RefreshAsync(string key, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();

            //using (MiniProfiler.Current.CustomTiming($"{nameof(RedisDistributedCache)}(Async)", $"EXPIRE \"{GetCacheKey(key)}\"", "Refresh"))
            {
                await GetAndRefreshAsync(key, getData: false, token: token);
            }
        }


        private byte[] GetAndRefresh(string key, bool getData)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            RedisValue[] results;
            if (getData)
            {
                results = _cache.HashGet(GetCacheKey(key), new RedisValue[] { AbsoluteExpirationKey, SlidingExpirationKey, DataKey });
            }
            else
            {
                results = _cache.HashGet(GetCacheKey(key), new RedisValue[] { AbsoluteExpirationKey, SlidingExpirationKey });
            }

            if (results.Length >= 2)
            {
                MapMetadata(results, out var absExpr, out var sldExpr);
                Refresh(key, absExpr, sldExpr);
            }

            if (results.Length >= 3 && results[2].HasValue)
            {
                return results[2];
            }

            return null;
        }

        private async Task<byte[]> GetAndRefreshAsync(string key, bool getData, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();

            RedisValue[] results;
            if (getData)
            {
                results = await _cache.HashGetAsync(GetCacheKey(key), new RedisValue[] { AbsoluteExpirationKey, SlidingExpirationKey, DataKey });
            }
            else
            {
                results = await _cache.HashGetAsync(GetCacheKey(key), new RedisValue[] { AbsoluteExpirationKey, SlidingExpirationKey });
            }

            if (results.Length >= 2)
            {
                MapMetadata(results, out var absExpr, out var sldExpr);
                await RefreshAsync(key, absExpr, sldExpr, token);
            }

            if (results.Length >= 3 && results[2].HasValue)
            {
                return results[2];
            }

            return null;
        }

        /// <inheritdoc />
        public void Remove(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            //using (MiniProfiler.Current.CustomTiming(nameof(RedisDistributedCache), $"DEL \"{GetCacheKey(key)}\"", "Remove"))
            {
                _cache.KeyDelete(GetCacheKey(key));
            }
        }

        /// <inheritdoc />
        public async Task RemoveAsync(string key, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            //using (MiniProfiler.Current.CustomTiming($"{nameof(RedisDistributedCache)}(Async)", $"DEL \"{GetCacheKey(key)}\"", "Remove"))
            {
                await _cache.KeyDeleteAsync(GetCacheKey(key));
            }
        }

        private string GetCacheKey(string key)
        {
            return string.Concat(_cachePrefix, key);
        }

        private void MapMetadata(RedisValue[] results, out DateTimeOffset? absoluteExpiration, out TimeSpan? slidingExpiration)
        {
            absoluteExpiration = null;
            slidingExpiration = null;
            var absoluteExpirationTicks = (long?)results[0];
            if (absoluteExpirationTicks.HasValue && absoluteExpirationTicks.Value != NotPresent)
            {
                absoluteExpiration = new DateTimeOffset(absoluteExpirationTicks.Value, TimeSpan.Zero);
            }
            var slidingExpirationTicks = (long?)results[1];
            if (slidingExpirationTicks.HasValue && slidingExpirationTicks.Value != NotPresent)
            {
                slidingExpiration = new TimeSpan(slidingExpirationTicks.Value);
            }
        }

        private void Refresh(string key, DateTimeOffset? absExpr, TimeSpan? sldExpr)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            // Note Refresh has no effect if there is just an absolute expiration (or neither).
            if (sldExpr.HasValue)
            {
                TimeSpan? expr;
                if (absExpr.HasValue)
                {
                    var relExpr = absExpr.Value - DateTimeOffset.Now;
                    expr = relExpr <= sldExpr.Value ? relExpr : sldExpr;
                }
                else
                {
                    expr = sldExpr;
                }
                _cache.KeyExpire(GetCacheKey(key), expr);
            }
        }

        private async Task RefreshAsync(string key, DateTimeOffset? absExpr, TimeSpan? sldExpr, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();

            // Note Refresh has no effect if there is just an absolute expiration (or neither).
            if (sldExpr.HasValue)
            {
                TimeSpan? expr;
                if (absExpr.HasValue)
                {
                    var relExpr = absExpr.Value - DateTimeOffset.Now;
                    expr = relExpr <= sldExpr.Value ? relExpr : sldExpr;
                }
                else
                {
                    expr = sldExpr;
                }
                await _cache.KeyExpireAsync(GetCacheKey(key), expr);
            }
        }

        private static TimeSpan? GetExpiration(DateTimeOffset creationTime, DateTimeOffset? absoluteExpiration, DistributedCacheEntryOptions options)
        {
            if (absoluteExpiration.HasValue && options.SlidingExpiration.HasValue)
            {
                var left = absoluteExpiration.Value - creationTime;
                var right = options.SlidingExpiration.Value;

                return left < right ? left : right;
            }

            if (absoluteExpiration.HasValue)
            {
                return absoluteExpiration.Value - creationTime;
            }

            return options.SlidingExpiration;
        }

        private static DateTimeOffset? GetAbsoluteExpiration(DateTimeOffset creationTime, DistributedCacheEntryOptions options)
        {
            if (options.AbsoluteExpiration.HasValue && options.AbsoluteExpiration <= creationTime)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(DistributedCacheEntryOptions.AbsoluteExpiration),
                    options.AbsoluteExpiration.Value,
                    "The absolute expiration value must be in the future.");
            }
            var absoluteExpiration = options.AbsoluteExpiration;
            if (options.AbsoluteExpirationRelativeToNow.HasValue)
            {
                absoluteExpiration = creationTime + options.AbsoluteExpirationRelativeToNow;
            }

            return absoluteExpiration;
        }
    }

}

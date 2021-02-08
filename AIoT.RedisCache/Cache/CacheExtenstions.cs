using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AIoT.Core.Cache;

namespace AIoT.RedisCache.Cache
{
    /// <summary>
    /// 缓存扩展方法
    /// </summary>
    public static class CacheExtenstions
    {
        /// <summary>
        /// 获取或更新缓存
        /// </summary>
        public static ValueTask<TData> GetOrAddAsync<TData>(
            this ICache<TData> cache,
            string key, Func<Task<TData>> factory,
            TimeSpan? absolute = null, TimeSpan? sliding = null
        )
        {
            var option = CreateCacheOptionsOrNull(absolute, sliding);
            return cache.GetOrAddAsync(key, factory, option != null ? () => option : (Func<CacheEntryOptions>)null);
        }

        /// <summary>
        /// 设置缓存
        /// </summary>
        public static Task SetAsync<TData>(
            this ICache<TData> cache,
            string key, TData value,
            TimeSpan? absolute = null, TimeSpan? sliding = null
        )
        {
            var option = CreateCacheOptionsOrNull(absolute, sliding);
            return cache.SetAsync(key, value, option);
        }

        /// <summary>
        /// 获取或更新缓存
        /// </summary>
        public static ValueTask<IReadOnlyDictionary<TId, TData>> GetOrAddAsync<TData, TId>(
            this IListCache<TData, TId> cache,
            string key, Func<Task<IDictionary<TId, TData>>> factory,
            TimeSpan? absolute = null, TimeSpan? sliding = null
        )
        {
            var option = CreateCacheOptionsOrNull(absolute, sliding);
            return cache.GetOrAddAsync(key, factory, option != null ? () => option : (Func<CacheEntryOptions>)null);
        }


        /// <summary>
        /// 获取指定缓存集合，如果不存在则返回 null，如果指定的 Field 不存在则 Data 为 默认值
        /// </summary>
        public static ValueTask<IEnumerable<TData>> GetByFieldsAsync<TId, TData>(
            this IListCache<TData, TId> cache,
            string key, params TId[] fields)
        {
            return cache.GetAllAsync(key, fields);
        }

        /// <summary>
        /// 设置缓存
        /// </summary>
        public static Task SetAsync<TData, TId>(
            this IListCache<TData, TId> cache,
            string key, IDictionary<TId, TData> values,
            TimeSpan? absolute = null, TimeSpan? sliding = null
        )
        {
            var option = CreateCacheOptionsOrNull(absolute, sliding);
            return cache.SetAsync(key, values, option);
        }

        /// <summary>
        /// 设置缓存
        /// </summary>
        public static Task SetAsync<TData, TId>(
            this IListCache<TData, TId> cache,
            string key, TId field, TData value,
            TimeSpan? absolute = null, TimeSpan? sliding = null
        )
        {
            var option = CreateCacheOptionsOrNull(absolute, sliding);
            return cache.SetAsync(key, field, value, option);
        }

        private static CacheEntryOptions CreateCacheOptionsOrNull(
            TimeSpan? absolute = null, TimeSpan? sliding = null)
        {
            if (absolute != null || sliding != null)
            {
                return new CacheEntryOptions()
                {
                    AbsoluteExpire = absolute,
                    SlidingExpire = sliding,
                };
            }

            return null;
        }
    }
}

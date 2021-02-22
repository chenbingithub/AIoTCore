using System.Collections.Generic;

namespace AIoT.RedisCache.Cache.Internal
{

    /// <summary>
    /// 本地缓存对像
    /// </summary>
    public class LocalCacheEntry<TData>
    {
        /// <summary>
        /// 本地缓存对像
        /// </summary>
        public LocalCacheEntry(TData data, CacheMeta meta)
        {
            Data = data;
            Meta = meta;
        }

        /// <summary>
        /// 缓存数据
        /// </summary>
        public TData Data { get; }

        /// <summary>
        /// 元数据
        /// </summary>
        public CacheMeta Meta { get; }
    }


    /// <summary>
    /// 本地缓存对像
    /// </summary>
    public class LocalCacheEntry<TKey, TData> : LocalCacheEntry<IReadOnlyDictionary<TKey, TData>>
    {
        /// <inheritdoc />
        public LocalCacheEntry(IReadOnlyDictionary<TKey, TData> data, CacheMeta meta) : base(data, meta)
        {
        }
    }
}
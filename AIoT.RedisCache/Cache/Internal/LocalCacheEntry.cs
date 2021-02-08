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
}
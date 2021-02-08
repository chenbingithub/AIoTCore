using AIoT.Core.Cache;

namespace AIoT.RedisCache.Cache
{
    /// <summary>
    /// 缓存管理
    /// </summary>
    public interface ICacheManager
    {
        /// <summary>
        /// 获取指定名称的 <see cref="ICache{TData}"/>
        /// </summary>
        ICache<TData> GetCache<TData>(string cacheName = null);

        /// <summary>
        /// 获取指定名称的 <see cref="IListCache{TId,TData}"/>
        /// </summary>
        IListCache<TData, TId> GetListCache<TData, TId>(string cacheName = null);
    }
}

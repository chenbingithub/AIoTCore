using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIoT.RedisCache.Cache
{
    /// <summary>
    /// 分布式集合缓存
    /// </summary>
    public interface IListCache<TData, TId>
    {
        /// <summary>
        /// 获取指定缓存，如果不存在则返回 默认值
        /// </summary>
        ValueTask<TData> GetAsync(string key, TId id);

        /// <summary>
        /// 获取缓存字典，如果不存在则返回 null
        /// </summary>
        ValueTask<IReadOnlyDictionary<TId, TData>> GetAsync(string key);

        /// <summary>
        /// 获取缓存集合，如果不存在则返回 null
        /// </summary>
        ValueTask<IEnumerable<TData>> GetAllAsync(string key);

        /// <summary>
        /// 获取指定缓存集合，如果不存在则返回 null，如果指定的 Id 不存在则 Data 为 null
        /// </summary>
        ValueTask<IEnumerable<TData>> GetAllAsync(string key, IEnumerable<TId> ids);

        /// <summary>
        /// 获取或添加缓存集合，如果 factory 返回 null 则不缓存
        /// </summary>
        ValueTask<IReadOnlyDictionary<TId, TData>> GetOrAddAsync(string key, Func<Task<IDictionary<TId, TData>>> factory,
            Func<CacheEntryOptions> optionsFactory = null);

        /// <summary>
        /// 获取或添加单个缓存，如果 factory 返回 null 则不缓存
        /// </summary>
        ValueTask<TData> GetOrAddAsync(string key, TId id, Func<TId, Task<TData>> factory,
            Func<CacheEntryOptions> optionsFactory = null);

        /// <summary>
        /// 添加缓存集合
        /// </summary>
        Task SetAsync(string key, IDictionary<TId, TData> values, CacheEntryOptions options = null);

        /// <summary>
        /// 添加单个缓存
        /// </summary>
        Task SetAsync(string key, TId id, TData value, CacheEntryOptions options = null);

        /// <summary>
        /// 刷新缓存滑动过期时间
        /// </summary>
        Task<bool> RefreshAsync(string key);

        /// <summary>
        /// 移除缓存
        /// </summary>
        Task<bool> RemoveAsync(string key);

        /// <summary>
        /// 移除指定缓存
        /// </summary>
        Task<bool> RemoveAsync(string key, TId id);
    }
}

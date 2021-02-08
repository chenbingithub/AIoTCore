using System;
using System.Threading.Tasks;
using AIoT.RedisCache.Cache;

namespace AIoT.Core.Cache
{
    /// <summary>
    /// 定义缓存接口
    /// </summary>
    public interface ICache<TData>
    {
        /// <summary>
        /// 获取缓存
        /// </summary>
        ValueTask<TData> GetAsync(string key);

        /// <summary>
        /// 获取或更新缓存
        /// </summary>
        ValueTask<TData> GetOrAddAsync(string key, Func<Task<TData>> factory,
            Func<CacheEntryOptions> optionsFactory = null);

        /// <summary>
        /// 设置缓存
        /// </summary>
        Task SetAsync(string key, TData value, CacheEntryOptions options = null);

        /// <summary>
        /// 刷新缓存滑动过期时间
        /// </summary>
        Task<bool> RefreshAsync(string key);

        /// <summary>
        /// 移除缓存
        /// </summary>
        Task<bool> RemoveAsync(string key);
    }
}

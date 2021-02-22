using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using StackExchange.Redis;

namespace AIoT.RedisCache.Cache.Internal
{
    /// <summary>
    /// 缓存存储类
    /// </summary>
    public interface ICacheStorage
    {
        /// <summary>
        /// 获取内存缓存
        /// </summary>
        IMemoryCache GetMemoryCache();

        /// <summary>
        /// 获取Redis缓存
        /// </summary>
        IDatabase GetRedisCache();

        /// <summary>
        /// 广播缓存过期
        /// </summary>
        void PublishCacheExpired(string key);

        /// <summary>
        /// 广播缓存过期
        /// </summary>
        Task PublishCacheExpiredAsync(string key);
    }
}

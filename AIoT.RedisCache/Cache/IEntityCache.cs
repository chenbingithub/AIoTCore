using System.Threading.Tasks;
using AIoT.Core.Cache;

namespace AIoT.RedisCache.Cache
{
    /// <summary>
    /// 实体缓存
    /// </summary>
    public interface IEntityCache<TCacheItem, in TKey>
    {
        /// <summary>
        /// 缓存服务
        /// </summary>
        ICache<TCacheItem> InternalCache { get; }

        /// <summary>
        /// 获取缓存对像
        /// </summary>
        ValueTask<TCacheItem> GetAsync(TKey id);
    }

    /// <summary>
    /// 实体缓存
    /// </summary>
    public interface IEntityCache<TCacheItem> : IEntityCache<TCacheItem, int>
    {
    }
}

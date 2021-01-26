using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIoT.Core.Cache
{
    /// <summary>
    /// 实体集合缓存
    /// </summary>
    public interface IEntityListCache<TCacheItem, TKey>
    {
        /// <summary>
        /// 获取缓存对像
        /// </summary>
        ValueTask<TCacheItem> GetAsync(TKey id);

        /// <summary>
        /// 获取所有缓存集合，如果不存在则返回 null
        /// </summary>
        ValueTask<IEnumerable<TCacheItem>> GetAllAsync();

        /// <summary>
        /// 获取指定缓存集合，如果不存在则返回 null，如果指定的 Id 不存在则 Data 为 默认值
        /// </summary>
        ValueTask<IEnumerable<TCacheItem>> GetAllAsync(IEnumerable<TKey> ids);

        /// <summary>
        /// 缓存服务
        /// </summary>
        IListCache<TCacheItem, TKey> InternalCache { get; }
    }

    /// <summary>
    /// 实体集合缓存
    /// </summary>
    public interface IEntityListCache<TCacheItem> : IEntityListCache<TCacheItem, int>
    {

    }
}

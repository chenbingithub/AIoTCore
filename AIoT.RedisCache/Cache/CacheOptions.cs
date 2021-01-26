using System;
using System.Collections.Generic;
using AIoT.Core.Cache.Internal;

namespace AIoT.Core.Cache
{
    /// <summary>
    /// 分布式缓存配置
    /// </summary>
    public class CacheOptions
    {
        private readonly Dictionary<string, CacheEntryOptions> _entryOptions;

        /// <summary>
        /// 分布式缓存配置
        /// </summary>
        public CacheOptions()
        {
            _entryOptions = new Dictionary<string, CacheEntryOptions>
            {
                [CacheBase.DefaultName] = DefaultCache
            };
        }
        
        /// <summary>
        /// 连接字符串
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Db
        /// </summary>
        public int DatabaseId { get; set; } = -1;

        /// <summary>
        /// Key前缀
        /// </summary>
        public string Prefix { get; set; } = "Cache";

        /// <summary>
        /// 缓存配置
        /// </summary>
        public IReadOnlyDictionary<string, CacheEntryOptions> EntryOptions => _entryOptions;

        /// <summary>
        /// 获取缓存配置
        /// </summary>
        public CacheEntryOptions GetOrDefaultOption(string cacheName)
        {
            return _entryOptions.TryGetValue(cacheName, out var option) ? option : DefaultCache;
        }

        /// <summary>
        /// 配置缓存
        /// </summary>
        public CacheOptions CacheOption(string cacheName, Action<CacheEntryOptions> configAction)
        {
            if (!_entryOptions.TryGetValue(cacheName, out var option))
            {
                _entryOptions.Add(cacheName, option = new CacheEntryOptions());
            }

            configAction(option);

            return this;
        }

        /// <summary>
        /// 缓存默认配置
        /// </summary>
        public CacheEntryOptions DefaultCache { get; } = new CacheEntryOptions();
    }
}
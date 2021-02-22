using System;
using System.Collections.Generic;
using AIoT.RedisCache.Cache.Internal;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace AIoT.RedisCache.Cache
{
    /// <summary>
    /// 分布式缓存配置
    /// </summary>
    public class CacheOptions
    {
        /// <summary>
        /// 默认缓存名称
        /// </summary>
        public const string DefaultCacheName = "Default";


        private readonly Dictionary<string, CacheEntryConfigOptions> _entryOptions;

        /// <summary>
        /// 分布式缓存配置
        /// </summary>
        public CacheOptions()
        {
            _entryOptions = new Dictionary<string, CacheEntryConfigOptions>
            {
                [DefaultCacheName] = DefaultCache
            };

            SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
            SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            SerializerSettings.Formatting = Formatting.None;
        }

        /// <summary>
        /// 连接字符串
        /// </summary>
        public IConnectionMultiplexer Connection { get; set; }

        /// <summary>
        /// Db
        /// </summary>
        public int Db { get; set; } = -1;

        /// <summary>
        /// Key前缀
        /// </summary>
        public string Prefix { get; set; } = "Cache";

        /// <summary>
        /// 序列化配置
        /// </summary>
        public JsonSerializerSettings SerializerSettings { get; } = new JsonSerializerSettings();

    

        /// <summary>
        /// 获取缓存配置
        /// </summary>
        public CacheEntryConfigOptions GetOrDefaultOption(string cacheName)
        {
            return _entryOptions.TryGetValue(cacheName, out var option) ? option : DefaultCache;
        }


        /// <summary>
        /// 缓存默认配置: 绝对过期时间：30分钟， 策略：内存+Redis
        /// </summary>
        public CacheEntryConfigOptions DefaultCache { get; } = new CacheEntryConfigOptions();
    }
}
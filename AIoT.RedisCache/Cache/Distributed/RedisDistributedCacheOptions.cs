using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace AIoT.RedisCache.Cache.Distributed
{
    /// <inheritdoc />
    public class RedisDistributedCacheOptions: IOptions<RedisDistributedCacheOptions>
    {
        /// <summary>
        /// 连接
        /// </summary>
        public IConnectionMultiplexer Connection { get; set; }

        /// <summary>键前缀</summary>
        public string Prefix { get; set; }

        /// <summary>数据库</summary>
        public int Db { get; set; } = -1;

        /// <inheritdoc />
        RedisDistributedCacheOptions IOptions<RedisDistributedCacheOptions>.Value => this;
    }
}

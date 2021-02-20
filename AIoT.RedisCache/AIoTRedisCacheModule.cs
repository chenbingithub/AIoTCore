using System;
using AIoT.Core;
using AIoT.RedisCache.Cache;
using AIoT.RedisCache.Cache.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Volo.Abp.Modularity;

namespace AIoT.RedisCache
{
    [DependsOn(typeof(AIoTCoreModule))]
    public class AIoTRedisCacheModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var config = context.Services.GetConfiguration();
            // Redis单例模式
            var redisConStr = config.GetConnectionString("Redis");
            var redisConnect = ConnectionMultiplexer.Connect(redisConStr);
            context.Services.AddSingleton<IConnectionMultiplexer>(redisConnect);
            context.Services.AddMemoryCache();
            context.Services.AddSingleton<ICacheStorage, CacheStorage>();

            // 缓存
            context.Services.AddRedisDistributedCache(p =>
            {
                p.Connection = redisConnect;
                p.Prefix = "AIoT:Cache";
            });

            // Cache，默认缓存时间：30分钟， 内存+Redis
            Configure<CacheOptions>(options =>
            {
                options.CacheOption(CacheOptions.DefaultCacheName, p =>
                {
                    p.AbsoluteExpire = TimeSpan.FromMinutes(30);
                    p.StoragePolicy = CacheStoragePolicy.MemoryAndRedis;
                });
            });
        }

    }
}

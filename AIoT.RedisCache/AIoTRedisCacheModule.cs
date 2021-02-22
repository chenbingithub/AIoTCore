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
           

            // 缓存
            context.Services.AddRedisDistributedCache(p =>
            {
                p.Connection = redisConnect;
                p.Prefix = "AIoT";
                p.DefaultCache.StoragePolicy = CacheStoragePolicy.Redis;
                p.DefaultCache.AbsoluteExpire = TimeSpan.FromMinutes(30);
            });

        }

    }
}

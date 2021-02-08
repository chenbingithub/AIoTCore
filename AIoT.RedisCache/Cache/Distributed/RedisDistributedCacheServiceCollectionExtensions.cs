using System;
using AIoT.RedisCache.Cache;
using AIoT.RedisCache.Cache.Distributed;
using AIoT.RedisCache.Cache.Internal;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary> </summary>
    public static class RedisDistributedCacheServiceCollectionExtensions
    {
     
        /// <summary>
        /// 注册 <see cref="RedisDistributedCache"/> 为 <see cref="IDistributedCache"/> 缓存服务
        /// </summary>
        public static IServiceCollection AddRedisDistributedCache(this IServiceCollection services, Action<CacheOptions> optionsAction)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (optionsAction == null)
            {
                throw new ArgumentNullException(nameof(optionsAction));
            }

            services.AddOptions();
            services.Configure(optionsAction);
            services.Add(ServiceDescriptor.Singleton<IDistributedCache, RedisDistributedCache>());
            services.AddSingleton<ICacheStorage, CacheStorage>();
            services.AddHostedService<CacheStorage>();


            return services;
        }
        /// <summary>
        /// 注册Redis和缓存
        /// </summary>
        public static void AddRedisAndCache(this IServiceCollection services, IConfiguration config)
        {
            // Redis单例模式
            var redisConStr = config.GetConnectionString("Redis");
            var redisConnect = ConnectionMultiplexer.Connect(redisConStr);
            services.AddSingleton<IConnectionMultiplexer>(redisConnect);

            // 缓存
            services.AddRedisDistributedCache(p =>
            {
                p.Connection = redisConnect;
                p.Prefix = "AIoT:Cache";
            });
        }
    }
}

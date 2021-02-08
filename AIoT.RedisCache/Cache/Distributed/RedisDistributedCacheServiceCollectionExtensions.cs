﻿using System;
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
        
    }
}

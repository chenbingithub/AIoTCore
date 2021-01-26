using System;
using AIoT.RedisCache.Cache.Distributed;
using Microsoft.Extensions.Caching.Distributed;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary> </summary>
    public static class RedisDistributedCacheServiceCollectionExtensions
    {
        /// <summary>
        /// 注册 <see cref="RedisDistributedCache"/> 为 <see cref="IDistributedCache"/> 缓存服务
        /// </summary>
        public static IServiceCollection AddRedisDistributedCache(this IServiceCollection services, Action<RedisDistributedCacheOptions> setupAction)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            services.AddOptions();
            services.Configure(setupAction);
            services.Add(ServiceDescriptor.Singleton<IDistributedCache, RedisDistributedCache>());

            return services;
        }
    }
}

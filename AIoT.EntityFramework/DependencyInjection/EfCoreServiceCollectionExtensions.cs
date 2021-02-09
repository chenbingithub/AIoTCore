using System;
using System.Collections.Generic;
using AIoT.EntityFramework.EfCore;
using AIoT.EntityFramework.EfCore.DependencyInjection;
using AIoT.EntityFramework.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AIoT.EntityFramework.DependencyInjection
{
    public static class EfCoreServiceCollectionExtensions
    {
        public static IServiceCollection AddAbpDbContext<TDbContext>(
            this IServiceCollection services)
            where TDbContext : AbpDbContext<TDbContext>
        {

            services.TryAddTransient(DbContextOptionsFactory.Create<TDbContext>);
           // services.AddDbContext<TDbContext, TDbContext>(ServiceLifetime.Transient, ServiceLifetime.Transient);

            services.Replace(ServiceDescriptor.Transient(typeof(TDbContext), typeof(TDbContext)));
            //添加仓储
            services.AddEfCoreRepository<TDbContext>();
            return services;
        }


        
    }
}

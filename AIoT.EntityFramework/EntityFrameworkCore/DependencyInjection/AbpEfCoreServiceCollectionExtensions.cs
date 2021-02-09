using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AIoT.EntityFramework.EntityFrameworkCore.DependencyInjection
{
    public static class AbpEfCoreServiceCollectionExtensions
    {
        public static IServiceCollection AddAbpDbContext<TDbContext>(
            this IServiceCollection services)
            where TDbContext : AbpDbContext<TDbContext>
        {
            //services.AddMemoryCache();

            //var options = new AbpDbContextRegistrationOptions(typeof(TDbContext), services);
            //optionsBuilder?.Invoke(options);
           
            services.TryAddTransient(DbContextOptionsFactory.Create<TDbContext>);
           // services.AddDbContext<TDbContext, TDbContext>(ServiceLifetime.Transient, ServiceLifetime.Transient);

            services.Replace(ServiceDescriptor.Transient(typeof(TDbContext), typeof(TDbContext)));

            //foreach (var dbContextType in options.ReplacedDbContextTypes)
            //{
            //    services.Replace(ServiceDescriptor.Transient(dbContextType, typeof(TDbContext)));
            //}

           // new EfCoreRepositoryRegistrar(options).AddRepositories();

            return services;
        }
    }
}

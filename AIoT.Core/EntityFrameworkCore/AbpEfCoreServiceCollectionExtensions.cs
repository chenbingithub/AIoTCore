using Microsoft.Extensions.DependencyInjection.Extensions;
using AIoT.Core.EntityFrameworkCore;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AbpEfCoreServiceCollectionExtensions
    {
        public static IServiceCollection AddAbpDbContext<TDbContext>(
            this IServiceCollection services
            //, Action<AbpDbContextOptions> options = null
            )
            where TDbContext : AbpDbContext<TDbContext>
        {
            //services.AddMemoryCache();

            

            services.TryAddTransient(DbContextOptionsFactory.Create<TDbContext>);

            services.AddDbContext<TDbContext>(ServiceLifetime.Transient, ServiceLifetime.Transient);
            



            return services;
        }
    }
}

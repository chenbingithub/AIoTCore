using System.Data;
using AIoT.Core.Data;
using AIoT.Core.Threading;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;
using AIoT.Core.Uow;
using Volo.Abp;
using Volo.Abp.Autofac;
using Volo.Abp.Data;
using Volo.Abp.Threading;

namespace AIoT.Core
{
    [DependsOn(typeof(AbpAutofacModule))]
    public class AIoTCoreModule : AbpModule
    {
        public override void PreConfigureServices(ServiceConfigurationContext context)
        {
            var services = context.Services;

            // Uow 拦截器
            services.OnRegistred(UnitOfWorkInterceptorRegistrar.RegisterIfNeeded);

            
        }
    

    public override void ConfigureServices(ServiceConfigurationContext context)
        {

            var services = context.Services;
            var config = services.GetConfiguration();
            context.Services.AddSingleton<ICancellationTokenProvider>(NullCancellationTokenProvider.Instance);
                Configure<AbpUnitOfWorkDefaultOptions>(options =>
            {
                options.IsTransactional = true;
                options.IsolationLevel = IsolationLevel.ReadCommitted;
            });

                Configure<AbpDbConnectionOptions>(config);
                Configure<AbpDataFilterOptions>(options =>
                {
                    options.DefaultStates[typeof(ISoftDelete)] = new DataFilterState(true);
                });
            context.Services.AddSingleton(typeof(IDataFilter<>), typeof(DataFilter<>));


        }
    }
   
}

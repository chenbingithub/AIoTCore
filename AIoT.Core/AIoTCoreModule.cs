using System;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;
using AIoT.Core.Uow;

namespace AIoT.Core
{
    public class AIoTCoreModule : AbpModule
    {
        public override void PreConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.OnRegistred(UnitOfWorkInterceptorRegistrar.RegisterIfNeeded);
        }
    }
   
}

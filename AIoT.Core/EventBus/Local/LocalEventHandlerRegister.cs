using System;
using AIoT.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;

namespace AIoT.Core.EventBus.Local
{
    public static class LocalEventHandlerRegister
    {
        /// <summary>
        /// 本地事件Handler注册
        /// </summary>
        public static void RegisterIfNeeded(IOnServiceExposingContext context, IServiceCollection service)
        {
            if (context.ImplementationType.IsAssignableTo(typeof(IEventHandler)))
            {
                var localEventHandler = typeof(ILocalEventHandler<>);

                var handlers = context.ImplementationType.GetClosedGenericTypes(localEventHandler);
                
                var options = new ConfigureNamedOptions<LocalEventBusOptions>(Options.DefaultName, 
                    p => p.Handlers[context.ImplementationType] = handlers);
                service.AddSingleton<IConfigureOptions<LocalEventBusOptions>>(options);
            }
        }
    }
}

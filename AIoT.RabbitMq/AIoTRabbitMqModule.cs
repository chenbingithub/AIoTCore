using System;
using AIoT.Core;
using AIoT.Core.Web;
using AIoT.RabbitMq.EasyNetQ;
using EasyNetQ.Consumer;
using EasyNetQ.DI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Volo.Abp;
using Volo.Abp.Modularity;

namespace AIoT.RabbitMq
{
    [DependsOn(typeof(AIoTCoreWebModule))]
    public class AIoTRabbitMqModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var config = context.Services.GetConfiguration();

            context.Services.AddSingleton<IServiceRegister, ServiceCollectionAdapter>();
            var rabbitMqConnection = config.GetConnectionString("RabbitMQ");
            context.Services.RegisterEasyNetQ(rabbitMqConnection);

            context.Services.Replace(ServiceDescriptor.Singleton<IConsumerErrorStrategy, ConsumerErrorStategy>());
        }

        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {
            var app = context.GetApplicationBuilder();
            //rabbitmq Subscribe
            app.UseRabbitMQSubscribe();
            base.OnApplicationInitialization(context);

        }
    }
}

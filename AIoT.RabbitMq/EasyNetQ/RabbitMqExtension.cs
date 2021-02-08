using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace AIoT.RabbitMq.EasyNetQ
{
    /// <inheritdoc />
    public static class RabbitMqExtension
    {
        /// <summary>
        /// 实现 rabbitmq 消费者订阅
        /// </summary>
        /// <param name="appBuilder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseRabbitMQSubscribe(this IApplicationBuilder appBuilder)
        {
            var services = appBuilder.ApplicationServices.CreateScope().ServiceProvider;
            services.GetServices<Consumer>().ToList().ForEach(async x => await x.InitSubscribeAsync());
            return appBuilder;
        }
    }
}

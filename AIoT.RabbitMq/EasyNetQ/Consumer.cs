using System;
using System.Threading.Tasks;
using EasyNetQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AIoT.RabbitMq.EasyNetQ
{
    /// <summary>
    /// RabbitMq消费者接口
    /// </summary>
    public abstract class Consumer : IConsumer
    {
        /// <summary>
        /// 日志
        /// </summary>
        public ILogger Logger { get; set; }
        private readonly IBus _bus;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="serviceProvider">服务提供程序</param>
        /// <param name="logger">日志</param>
        public Consumer(IServiceProvider serviceProvider, ILogger logger)
        {
            Logger = logger;
            _bus = serviceProvider.GetService<IBus>();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="mqName">消息队列名称，在连接字符串中</param>
        /// <param name="serviceProvider">服务提供程序</param>
        /// <param name="logger">日志</param>
        public Consumer(string mqName, IServiceProvider serviceProvider, ILogger logger)
        {
            if (string.IsNullOrEmpty(mqName))
                mqName = null;
            var busList = serviceProvider.GetServices<IBus>();
            foreach (var bus in busList)
            {
                if (bus.Advanced.Container.Resolve<IConnectionFactory>().Configuration.Name == mqName)
                {
                    _bus = bus;
                    break;
                }
            }
            Logger = logger;
        }

        /// <summary>
        /// 获取Bus对象
        /// </summary>
        /// <returns></returns>
        public virtual IBus GetBus()
        {
            return _bus;
        }

        /// <summary>
        /// 订阅
        /// </summary>
        /// <returns></returns>
        public virtual Task InitSubscribeAsync()
        {
            return Task.CompletedTask;
        }
    }

    //< inherit doc />
    public interface IConsumer
    {
        //< inherit doc />
        Task InitSubscribeAsync();
    }
}

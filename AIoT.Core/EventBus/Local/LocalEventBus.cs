using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AIoT.Core.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;

namespace AIoT.Core.EventBus.Local
{
    /// <summary>
    /// 进程内事件总线
    /// </summary>
    public class LocalEventBus : ILocalEventBus, ITransientDependency
    {
        private readonly LocalEventBusOptions _options;
        private readonly IServiceProvider _serviceProvider;

        public LocalEventBus(IOptions<LocalEventBusOptions> options, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _options = options.Value;
        }


        /// <inheritdoc />
        public async Task PublishAsync<TEventData>(TEventData eventData, CancellationToken cancellationToken = default)
        {
            await PublishCore(eventData, cancellationToken);
        }


        private async Task PublishCore<TEventData>(TEventData eventData, CancellationToken cancellation)
        {
            var dataType = typeof(TEventData);

            foreach (var (handler, interfaces) in _options.Handlers)
            {
                var handlerTypes = interfaces.Where(p => p.GenericTypeArguments[0].IsAssignableFrom(dataType)).ToArray();
                if (handlerTypes.Any())
                {
                    var ins = _serviceProvider.GetRequiredService(handler);
                    foreach (var handlerType in handlerTypes)
                    {
                        var handle = GetHandle(handlerType);
                        await handle(ins, eventData, cancellation);
                    }
                }
            }
        }

        #region 私有方法

        /// <summary>
        /// 获取指定类型的 <see cref="ILocalEventHandler{TEvent}.HandleAsync"/> 方法的委托。
        /// </summary>
        /// <param name="handlerType">指定 <see cref="ILocalEventHandler{TEvent}"/>接口类型。</param>
        private static HandleDelegate GetHandle(Type handlerType)
        {
            var eventDataType = handlerType.GenericTypeArguments[0];
            var handler = _cacheHandles.GetOrAdd(eventDataType,
                p => (HandleDelegate)
                    _methodInfo.MakeGenericMethod(p).CreateDelegate(
                        typeof(HandleDelegate)));
            return handler;
        }
        private delegate Task HandleDelegate(object handler, object eventData, CancellationToken cancellation);
        private static readonly ConcurrentDictionary<Type, HandleDelegate> _cacheHandles =
            new ConcurrentDictionary<Type, HandleDelegate>();

        private static readonly MethodInfo _methodInfo = typeof(LocalEventBus)
            .GetMethod("HandleAsync", BindingFlags.Static | BindingFlags.NonPublic);

        // ReSharper disable once UnusedMember.Local
        private static async Task HandleAsync<TEventData>(object handler,
            object eventData, CancellationToken token)
        {
            await ((ILocalEventHandler<TEventData>)handler).HandleEventAsync((TEventData)eventData, token);
        }

        #endregion

    }
}

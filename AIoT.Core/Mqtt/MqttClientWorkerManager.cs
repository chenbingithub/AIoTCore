using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Volo.Abp.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Threading;

namespace AIoT.Core.Mqtt
{
    /// <summary>
    /// 
    /// </summary>
    public class MqttClientWorkerManager : IMqttClientWorkerManager, IHostedService, ISingletonDependency, IDisposable
    {
        private readonly ConcurrentDictionary<string, MqttClientWorker> _clients;
        private readonly IServiceProvider _serviceProvider;
        protected bool IsRunning { get; private set; }

        private bool _isDisposed;

        public MqttClientWorkerManager(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _clients = new ConcurrentDictionary<string, MqttClientWorker>();
        }

        public  void Add(MqttOptions option)
        {
            Remove(option.TopicPrefix);
            var handler = _serviceProvider.GetService<IMessageReceivedHandler>();
            var client = new MqttClientWorker(option, handler);
            
            _clients.TryAdd(option.TopicPrefix, client);
            if (IsRunning)
            {
                AsyncHelper.RunSync(
                    () =>  client.StartAsync()
                );
                ;
            }
        }
        public  void Remove(string topicPrefix)
        {
            _clients.TryRemove(topicPrefix,out var client);
            if (client != null)
            {
                if (IsRunning)
                {
                    AsyncHelper.RunSync(
                        () => client.StopAsync()
                    );
                    ;
                }
            }
               
        }

        public async Task PublishAsync(string topicPrefix,string topic,string payload)
        {
            _clients.TryGetValue(topicPrefix, out var client);
            await client.PublishAsync($"{topicPrefix}/{topic}", payload);
        }
        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            IsRunning = true;
            foreach (var client in _clients)
            {
                await client.Value.StopAsync(cancellationToken);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            IsRunning = false;
            foreach (var client in _clients)
            {
                await client.Value.StopAsync(cancellationToken);
            }
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
        }
    }
}

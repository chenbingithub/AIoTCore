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

namespace AIoT.Core.Mqtt
{
    /// <summary>
    /// 
    /// </summary>
    public class MqttClientManager: IMqttClientManager,IHostedService, ISingletonDependency
    {
        private readonly ConcurrentDictionary<string, MqttClientService> _clients;
        private readonly IServiceProvider _serviceProvider;

        public MqttClientManager(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _clients = new ConcurrentDictionary<string, MqttClientService>();
        }

        public async Task AddAsync(MqttOptions option, CancellationToken cancellationToken = default)
        {
            await RemoveAsync(option.TopicPrefix);
            var handler = _serviceProvider.GetService<IMessageReceivedHandler>();
            var client = new MqttClientService(option, handler);
            await client.StartAsync(cancellationToken);
            _clients.TryAdd(option.TopicPrefix, client);
        }
        public async Task RemoveAsync(string topicPrefix, CancellationToken cancellationToken=default)
        {
            _clients.TryRemove(topicPrefix,out var client);
            if (client != null) await client.StopAsync(cancellationToken);
        }

        public async Task PublishAsync(string topicPrefix,string topic,string payload)
        {
            _clients.TryGetValue(topicPrefix, out var client);
            await client.PublishAsync($"{topicPrefix}/{topic}", payload);
        }
        public  Task StartAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            foreach (var client in _clients)
            {
                await client.Value.StopAsync(cancellationToken);
            }
        }
    }
}

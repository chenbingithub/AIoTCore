using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;

namespace AIoT.Core.Mqtt
{
    /// <summary>
    /// 缓存存储类
    /// </summary>
    public class MqttClientWorker :  IHostedService, IDisposable
    {
        private bool _isDisposed;
        private IMqttClientOptions _clientOptions;
        private IMqttClient _mqttClient;
        private MqttOptions _options;
        private IMessageReceivedHandler _messageReceivedHandler;
        public MqttClientWorker(MqttOptions mqttOptions, IMessageReceivedHandler messageReceivedHandler)
        {
            _options = mqttOptions;
            _messageReceivedHandler = messageReceivedHandler;
            _clientOptions = CreateMqttClientOptions(mqttOptions);
            _mqttClient = new MqttFactory().CreateMqttClient();
        }
        public async Task StartAsync(CancellationToken cancellationToken=default)
        {
            try
            {
                //断开事件
                _mqttClient.UseDisconnectedHandler(async e =>
                {
                    
                });
                //连接成功的事件
                _mqttClient.UseConnectedHandler(async e =>
                {
                    //订阅心跳消息
                    await _mqttClient.SubscribeAsync($"{_options.TopicPrefix}/heartbeat");
                    //所有设备
                    await _mqttClient.SubscribeAsync($"{_options.TopicPrefix}/data/post");
                    //单个设备
                    await _mqttClient.SubscribeAsync($"{_options.TopicPrefix}/data/+/post");
                  
                });
                //接收消息事件
                _mqttClient.UseApplicationMessageReceivedHandler(async e =>
                {
                     await _messageReceivedHandler.HandleEventAsync(e);
                });
                
                await TryConnect(cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"连接到MQTT服务器失败！{ex.Message}");
            }
        }

        /// <inheritdoc />
        public async Task StopAsync(CancellationToken cancellationToken=default)
        {

            if (_isDisposed) return;
            await _mqttClient.DisconnectAsync(cancellationToken);
            _mqttClient.Dispose();
            Dispose();
        }
        public async Task PublishAsync(string topic, string payload)
        {
            await _mqttClient.PublishAsync(topic, payload);
        }
        private IMqttClientOptions CreateMqttClientOptions(MqttOptions options)
        {
            //连接到服务器前，获取所需要的MqttClientTcpOptions 对象的信息
            var optionBuild = new MqttClientOptionsBuilder()
                .WithClientId(options.ClientId)
                .WithTcpServer(options.HostIp, options.HostPort)
                .WithCredentials(options.UserName, options.Password)
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(2000))
                .WithCleanSession();
            if (options.EnableSsl)
            {
                optionBuild.WithTls();
            }
            return optionBuild.Build();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_isDisposed) return;
            _clientOptions = null;
            _options = null;
            _isDisposed = true;
        }

        private async Task TryConnect(CancellationToken stoppingToken)
        {
            var success = false;
            var retryIndex = 1;
            while (!success)
            {
                try
                {
                    await _mqttClient.ConnectAsync(_clientOptions);
                    success = true;
                    Console.WriteLine($"已成功连接MQTT{_options.HostIp}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine( $"Error：{ex.Message} RetryCount：{retryIndex}");

                    retryIndex++;
                    await Task.Delay(1000, stoppingToken);
                }
            }
        }

        
    }
}

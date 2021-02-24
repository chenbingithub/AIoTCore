using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using Volo.Abp.DependencyInjection;

namespace AIoT.Core.Mqtt
{
    public class DefaultMessageReceivedHandler: IMessageReceivedHandler,ITransientDependency
    {
        public Task HandleEventAsync(MqttApplicationMessageReceivedEventArgs args)
        {
           return Task.CompletedTask;
        }
    }
}
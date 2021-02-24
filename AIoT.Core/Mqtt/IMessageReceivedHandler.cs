using System.Threading;
using System.Threading.Tasks;
using MQTTnet;

namespace AIoT.Core.Mqtt
{
    //TODO: Move to the right namespace in v3.0
    public interface IMessageReceivedHandler
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        Task HandleEventAsync(MqttApplicationMessageReceivedEventArgs args);
    }
}
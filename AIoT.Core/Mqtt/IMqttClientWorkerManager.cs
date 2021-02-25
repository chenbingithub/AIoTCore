using System.Threading;
using System.Threading.Tasks;

namespace AIoT.Core.Mqtt
{
    /// <summary>
    /// 
    /// </summary>
    public interface IMqttClientWorkerManager
    {
        void Add(MqttOptions option);
        void Remove(string topicPrefix);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="topicPrefix"></param>
        /// <param name="topic"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        Task PublishAsync(string topicPrefix, string topic, string payload);
    }
}

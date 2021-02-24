using System.Threading;
using System.Threading.Tasks;

namespace AIoT.Core.Mqtt
{
    /// <summary>
    /// 
    /// </summary>
    public interface IMqttClientManager
    {
        Task AddAsync(MqttOptions option, CancellationToken cancellationToken = default);
        Task RemoveAsync(string topicPrefix, CancellationToken cancellationToken = default);
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

using System.Threading;
using System.Threading.Tasks;

namespace AIoT.Core.EventBus.Local
{
    /// <summary>
    /// Defines interface of the event bus.
    /// </summary>
    public interface ILocalEventBus 
    {
        /// <summary>���������¼�</summary>
        Task PublishAsync<TEventData>(TEventData eventData, CancellationToken cancellationToken = default);
    }
    
}
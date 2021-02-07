using System.Threading;
using System.Threading.Tasks;

namespace AIoT.Core.EventBus.Local
{
    //TODO: Move to the right namespace in v3.0
    public interface ILocalEventHandler<in TEvent> : IEventHandler
    {
        /// <summary>
        /// Handler handles the event by implementing this method.
        /// </summary>
        /// <param name="eventData">Event data</param>
        /// <param name="cancellation"></param>
        Task HandleEventAsync(TEvent eventData,CancellationToken cancellation);
    }
}
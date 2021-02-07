using System.Threading;
using System.Threading.Tasks;

namespace AIoT.Core.EventBus.Local
{
  
    public class NullLocalEventBus : ILocalEventBus
    {
        public static readonly ILocalEventBus Instanse = new NullLocalEventBus();

        /// <inheritdoc />
        public Task PublishAsync<TEventData>(TEventData eventData, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
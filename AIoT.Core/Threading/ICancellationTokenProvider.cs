using System.Threading;

namespace AIoT.Core.Threading
{
    public interface ICancellationTokenProvider
    {
        CancellationToken Token { get; }
    }
}

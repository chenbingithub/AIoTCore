using System.Threading;
using Volo.Abp.Threading;

namespace AIoT.Core.Threading
{
    public static class CancellationTokenProviderExtensions
    {
        public static CancellationToken FallbackToProvider(this ICancellationTokenProvider provider, CancellationToken prefferedValue = default)
        {
            return prefferedValue == default || prefferedValue == CancellationToken.None
                ? provider.Token
                : prefferedValue;
        }
    }
}
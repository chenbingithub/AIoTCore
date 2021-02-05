using System.Linq;
using IdentityModel;
using Volo.Abp.DependencyInjection;

namespace AIoT.Core.Runtime.Internal
{
    public class CurrentClient : ICurrentClient, ITransientDependency
    {
        public string BrowserInfo { get; }
        public string ClientIpAddress { get; }
    }
}

using System.Linq;
using IdentityModel;
using Volo.Abp.DependencyInjection;

namespace AIoT.Core.Runtime.Internal
{
    public class CurrentClient : ICurrentClient, ITransientDependency
    {
        public virtual string Id => _principalAccessor.Principal?.Claims?.FirstOrDefault(c => c.Type == JwtClaimTypes.ClientId)?.Value;

        public virtual bool IsAuthenticated => Id != null;

        private readonly ICurrentPrincipalAccessor _principalAccessor;

        public CurrentClient(ICurrentPrincipalAccessor principalAccessor)
        {
            _principalAccessor = principalAccessor;
        }
    }
}

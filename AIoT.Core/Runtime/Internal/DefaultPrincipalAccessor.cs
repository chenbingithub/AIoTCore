using System.Security.Claims;
using System.Threading;
using Volo.Abp.DependencyInjection;

namespace AIoT.Core.Runtime.Internal
{
    /// <inheritdoc cref="IPrincipalAccessor" />
    public class DefaultPrincipalAccessor : IPrincipalAccessor, ISingletonDependency
    {
        /// <inheritdoc />
        public virtual ClaimsPrincipal Principal => Thread.CurrentPrincipal as ClaimsPrincipal;

        /// <summary>
        /// 单例模式
        /// </summary>
        public static DefaultPrincipalAccessor Instance => new DefaultPrincipalAccessor();
    }
}
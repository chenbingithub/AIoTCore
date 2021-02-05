using System.Security.Claims;
using AIoT.Core.Runtime.Internal;
using Microsoft.AspNetCore.Http;
using Volo.Abp.DependencyInjection;

namespace AIoT.Core.Web.Runtime
{
    /// <summary>
    /// 从 <see cref="HttpContext"/> 中获取当前用户凭证
    /// </summary>
    [Dependency(ReplaceServices = true)]
    public class AspNetCorePrincipalAccessor : CurrentPrincipalAccessorBase
    {

        protected override ClaimsPrincipal GetClaimsPrincipal()
        {
          return  _httpContextAccessor.HttpContext?.User ?? base.Principal;
        }


        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <inheritdoc />
        public AspNetCorePrincipalAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
    }
}

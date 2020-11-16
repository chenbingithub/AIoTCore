using System.Security.Claims;
using AIoT.Core.Runtime.Internal;
using Microsoft.AspNetCore.Http;

namespace AIoT.Core.Web.Runtime
{
    /// <summary>
    /// 从 <see cref="HttpContext"/> 中获取当前用户凭证
    /// </summary>
    public class AspNetCorePrincipalAccessor : DefaultPrincipalAccessor
    {
        /// <inheritdoc />
        public override ClaimsPrincipal Principal => _httpContextAccessor.HttpContext?.User ?? base.Principal;


        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <inheritdoc />
        public AspNetCorePrincipalAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
    }
}

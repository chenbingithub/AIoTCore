using System.Security.Claims;

namespace AIoT.Core.Runtime
{
    /// <summary>
    /// 获取当前身份凭证 <see cref="ClaimsPrincipal"/> 接口
    /// </summary>
    public interface IPrincipalAccessor
    {
        /// <summary>
        /// 获取当前身份凭证
        /// </summary>
        ClaimsPrincipal Principal { get; }
    }
}
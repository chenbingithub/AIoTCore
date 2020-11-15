using System.Collections.Generic;
using System.Security.Claims;
using IdentityModel;
using Volo.Abp.DependencyInjection;

namespace AIoT.Core.Runtime.Internal
{
    /// <summary>
    /// 使用 <see cref="ClaimsPrincipal"/> 获取当前用户
    /// </summary>
    public class DefaultCurrentUser : ICurrentUser, ITransientDependency
    {
        /// <summary>
        /// 获取用户Id
        /// </summary>
        public string UserId => _principalAccessor.Principal?.Claims.GetValue(JwtClaimTypes.Subject);

        /// <summary>
        /// 获取登录名称（工号）
        /// </summary>
        public string UserName => _principalAccessor.Principal?.Claims.GetValue(JwtClaimTypes.Name);

        /// <summary>
        /// 获取姓名
        /// </summary>
        public string Name => _principalAccessor.Principal?.Claims.GetValue(JwtClaimTypes.NickName);

        /// <summary>
        /// 获取姓名
        /// </summary>
        public string ClientId => _principalAccessor.Principal?.Claims.GetValue(JwtClaimTypes.ClientId);

        /// <inheritdoc />
        public IEnumerable<Claim> Claims => _principalAccessor.Principal?.Claims;


        private readonly IPrincipalAccessor _principalAccessor;

        /// <summary>
        /// 创建 <see cref="DefaultCurrentUser"/>
        /// </summary>
        public DefaultCurrentUser(IPrincipalAccessor principalAccessor)
        {
            _principalAccessor = principalAccessor;
        }
    }
}

using System;
using System.Linq;
using System.Security.Claims;
using IdentityModel;
using Volo.Abp.DependencyInjection;

namespace AIoT.Core.Runtime.Internal
{
    public class CurrentUser : ICurrentUser, ITransientDependency
    {

        /// <summary>
        /// 获取用户Id
        /// </summary>
        public virtual string UserId => this.FindClaimValue(JwtClaimTypes.Subject);

        /// <summary>
        /// 获取登录名称（工号）
        /// </summary>
        public virtual string UserName => this.FindClaimValue(JwtClaimTypes.Name);

        /// <summary>
        /// 获取姓名
        /// </summary>
        public virtual string Name => this.FindClaimValue(JwtClaimTypes.NickName);

        /// <summary>
        /// 获取姓名
        /// </summary>
        public virtual string ClientId => this.FindClaimValue(JwtClaimTypes.ClientId);

        

       


        private readonly ICurrentPrincipalAccessor _principalAccessor;

        public CurrentUser(ICurrentPrincipalAccessor principalAccessor)
        {
            _principalAccessor = principalAccessor;
        }

        public virtual Claim FindClaim(string claimType)
        {
            return _principalAccessor.Principal?.Claims.FirstOrDefault(c => c.Type == claimType);
        }

        public virtual Claim[] FindClaims(string claimType)
        {
            return _principalAccessor.Principal?.Claims.Where(c => c.Type == claimType).ToArray() ?? new Claim[0];
        }

        public virtual Claim[] GetAllClaims()
        {
            return _principalAccessor.Principal?.Claims.ToArray()  ?? new Claim[0];
        }

       
    }
}

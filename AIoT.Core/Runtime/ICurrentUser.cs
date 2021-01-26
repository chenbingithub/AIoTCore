using System;
using System.Security.Claims;
using JetBrains.Annotations;

namespace AIoT.Core.Runtime
{
    public interface ICurrentUser
    {

        /// <summary>
        /// 获取用户Id
        /// </summary>
        string UserId { get; }

        /// <summary>
        /// 获取登录名
        /// </summary>
        string UserName { get; }

        /// <summary>
        /// 获取姓名
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 请求客户端
        /// </summary>
        string ClientId { get; }
        [CanBeNull]
        Claim FindClaim(string claimType);

        [NotNull]
        Claim[] FindClaims(string claimType);

        [NotNull]
        Claim[] GetAllClaims();

    }
}

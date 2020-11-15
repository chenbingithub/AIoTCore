using System.Collections.Generic;
using System.Security.Claims;

namespace AIoT.Core.Runtime
{
    /// <summary>
    /// 当前用户
    /// </summary>
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

        /// <summary>
        /// 获取当前会话的声明
        /// </summary>
        IEnumerable<Claim> Claims { get; }
    }
}

using System.Collections.Generic;
using System.Security.Claims;

namespace AIoT.Core.Runtime
{
    /// <inheritdoc />
    public class NullCurrentUser : ICurrentUser
    {
        /// <summary>
        /// 单例模式
        /// </summary>
        public static NullCurrentUser Instanse = new NullCurrentUser();

        /// <inheritdoc />
        public string UserId { get; } = null;

        /// <inheritdoc />
        public string UserName { get; } = null;

        /// <inheritdoc />
        public string Name { get; } = null;

        /// <summary>
        /// 请求客户端
        /// </summary>
        public string ClientId { get; } = null;

        /// <inheritdoc />
        public IEnumerable<Claim> Claims { get; } = new Claim[0];
    }
}

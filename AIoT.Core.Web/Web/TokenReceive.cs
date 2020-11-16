using System;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace AIoT.Core.Web
{
    /// <summary>
    /// Token 读取方式
    /// </summary>
    public class TokenReceive
    {
        /// <summary>
        /// 从多个指定的方式获取Token
        /// </summary>
        public static Func<HttpRequest, string> FromMultiple(params Func<HttpRequest, string>[] receivers)
        {
            return (request) =>
            {
                foreach (var receiver in receivers)
                {
                    var token = receiver(request);
                    if (!string.IsNullOrEmpty(token))
                    {
                        return token;
                    }
                }

                return null;
            };
        }

        /// <summary>
        /// 从Authorization头获取Token
        /// </summary>
        public static Func<HttpRequest, string> FromAuthorizationHeader(string scheme = "Bearer")
        {
            return (request) =>
            {
                var authorization = request.Headers["Authorization"].FirstOrDefault();

                if (string.IsNullOrEmpty(authorization))
                {
                    return null;
                }

                if (authorization.StartsWith(scheme + " ", StringComparison.OrdinalIgnoreCase))
                {
                    return authorization.Substring(scheme.Length + 1).Trim();
                }

                return null;
            };
        }

        /// <summary>
        /// 从Url参数中获取Token
        /// </summary>
        public static Func<HttpRequest, string> FromQueryString(string name = "access_token")
        {
            return (request) => request.Query[name].FirstOrDefault();
        }
    }
}

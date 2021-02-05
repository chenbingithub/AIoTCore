using System;
using AIoT.Core.Runtime;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;

namespace AIoT.Core.Web.Runtime
{
    /// <summary>
    /// 从Http请求中获取客户端信息
    /// </summary>
    [Dependency(ReplaceServices = true)]
    public class HttpContextClientInfo : ICurrentClient, ITransientDependency
    {
        private readonly HttpContext _httpContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<HttpContextClientInfo> _logger;

        /// <inheritdoc />
        public string BrowserInfo => GetBrowserInfo();

        /// <inheritdoc />
        public string ClientIpAddress => GetClientIpAddress();

        /// <summary>
        /// Creates a new <see cref="HttpContextClientInfo"/>.
        /// </summary>
        public HttpContextClientInfo(IHttpContextAccessor httpContextAccessor, ILogger<HttpContextClientInfo> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _httpContext = httpContextAccessor.HttpContext;

        }

        /// <summary>
        /// 获取客户端浏览器信息
        /// </summary>
        protected virtual string GetBrowserInfo()
        {
            var httpContext = _httpContextAccessor.HttpContext ?? _httpContext;
            return httpContext?.Request?.Headers?["User-Agent"];
        }

        /// <summary>
        /// 获取客户端Ip地址
        /// </summary>
        protected virtual string GetClientIpAddress()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext ?? _httpContext;
                return httpContext?.Connection?.RemoteIpAddress?.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"获取客户端Ip失败:{ex.Message}");
            }

            return null;
        }
    }
}

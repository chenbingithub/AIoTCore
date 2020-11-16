using IdentityModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace AIoT.Core.Web
{
    /// <summary>
    /// 请求日志
    /// </summary>
    [ExposeServices(typeof(HttpLogMiddleware))]
    public class HttpLogMiddleware : IMiddleware, ITransientDependency
    {
        private readonly ILoggerFactory _loggerFactory;

        /// <summary>
        /// 参数Json序列化配置
        /// </summary>
        private static JsonSerializerSettings SerializerSettings { get; }

        /// <summary>
        /// 要记录的 Body 类型
        /// </summary>
        private static IList<MediaTypeHeaderValue> LogMediaTypes { get; }

        static HttpLogMiddleware()
        {
            LogMediaTypes = new List<MediaTypeHeaderValue>()
            {
                new MediaTypeHeaderValue("text/json"),
                new MediaTypeHeaderValue("text/xml"),
                new MediaTypeHeaderValue("application/json"),
                new MediaTypeHeaderValue("application/json-patch+json"),
                new MediaTypeHeaderValue("application/*+json"),
                new MediaTypeHeaderValue("application/x-www-form-urlencoded"),
            };

            SerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver(),
                Formatting = Formatting.None,
                NullValueHandling = NullValueHandling.Include,
                DateTimeZoneHandling = DateTimeZoneHandling.Local,
                Converters =
                {
                    new IsoDateTimeConverter
                    {
                        DateTimeFormat = "yyyy-MM-dd HH:mm:ss.fffffff",
                        DateTimeStyles = DateTimeStyles.AssumeLocal
                    }
                }
            };
        }

        /// <inheritdoc />
        public HttpLogMiddleware(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        /// <inheritdoc />
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var request = context.Request;
            var headers = context.Request.GetTypedHeaders();
            var requestBody = string.Empty;
            var responseBody = string.Empty;
            Exception exception = null;

            var watch = Stopwatch.StartNew();

            try
            {
                if (headers.ContentType != null && LogMediaTypes.Any(p => headers.ContentType.IsSubsetOf(p)))
                {
                    if (!context.Request.Body.CanSeek)
                    {
                        context.Request.EnableBuffering();
                        await request.Body.DrainAsync(CancellationToken.None);
                        request.Body.Seek(0L, SeekOrigin.Begin);
                    }

                    var encoding = headers.ContentType.Encoding ?? SelectCharacterEncoding(request.ContentType);
                    using (var reader = new StreamReader(request.Body, encoding, false, 4096, true))
                    {
                        requestBody = await reader.ReadToEndAsync();
                    }

                    request.Body.Seek(0L, SeekOrigin.Begin);
                }

                await next(context);
                watch.Stop();
            }
            catch (Exception e)
            {
                exception = e;
                e.ReThrow();
            }
            finally
            {
                watch.Stop();

                if (context.Items.TryGetValue("ActionDescriptor", out var c1) &&
                    c1 is ControllerActionDescriptor actionDescriptor)
                {
                    var logName = $"{actionDescriptor.ControllerTypeInfo.FullName}.{actionDescriptor.MethodInfo.Name}";
                    var logger = _loggerFactory.CreateLogger(logName);

                    var result = context.Items.TryGetValue("ActionResult", out var c3) ? (IActionResult)c3 : null;
                    if (result is ObjectResult objectResult)
                    {
                        responseBody = JsonConvert.SerializeObject(objectResult.Value, SerializerSettings);
                    }

                    var auth = request.Headers["Authorization"];
                    var userId = context.User.FindFirst(JwtClaimTypes.Subject)?.Value;
                    var userName = context.User.FindFirst(JwtClaimTypes.Name)?.Value;
                    var name = context.User.FindFirst(JwtClaimTypes.NickName)?.Value;

                    if (exception == null && context.Items.TryGetValue("ActionException", out var ex))
                    {
                        exception = ex as Exception;
                    }

                    var logLevel = exception != null ? LogLevel.Error : LogLevel.Information;

                    logger.Log(logLevel, exception,
                        "请求：{Protocol} {Method} {RequestUrl} {RequestContentType}\r\n" +
                        "用户：UserName={UserName} Name={Name} UserId={UserId}\r\n" +
                        "{RequestBody}\r\n" +
                        "响应：{ElapsedMilliseconds}ms {StatusCode} {ResponseContentType}\r\n" +
                        "{ResponseBody}\r\n" +
                        "{ErrorMessage}",
                        context.Request.Protocol,
                        context.Request.Method,
                        context.Request.GetDisplayUrl(),
                        headers.ContentType?.ToString() ?? string.Empty,
                        userName ?? string.Empty, name ?? string.Empty, userId ?? string.Empty,
                        requestBody,
                        watch.Elapsed.TotalMilliseconds,
                        context.Response.StatusCode,
                        context.Response.ContentType,
                        responseBody,
                        exception?.Message ?? string.Empty);
                }
            }
        }



        private Encoding SelectCharacterEncoding(string contentType)
        {
            var mediaType = contentType == null ? new MediaType() : new MediaType(contentType);

            return mediaType.Encoding ?? Encoding.ASCII;
        }

        /// <inheritdoc />
        public class HttpLogFilter : ActionFilterAttribute
        {
            /// <inheritdoc />
            public HttpLogFilter()
            {
                Order = -1000000;
            }


            /// <inheritdoc />
            public override void OnActionExecuting(ActionExecutingContext context)
            {
                context.HttpContext.Items.Add("ActionDescriptor", context.ActionDescriptor);
            }

            /// <inheritdoc />
            public override void OnActionExecuted(ActionExecutedContext context)
            {
                context.HttpContext.Items.Add("ActionResult", context.Result);
            }
        }
    }


}

using System;
using System.IO;
using System.Text;
using System.Threading;
using AIoT.Core.Dto;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace AIoT.Core.Web
{
    /// <summary>
    /// 异常返回信息
    /// </summary>
    public class ExceptionResultFilter : ExceptionFilterAttribute
    {
        private readonly IHostingEnvironment _env;

        /// <inheritdoc />
        public ExceptionResultFilter(IHostingEnvironment env)
        {
            _env = env;
        }

        /// <inheritdoc />
        public override void OnException(ExceptionContext context)
        {
            if (context.Exception is OperationCanceledException ||
                context.Exception is ThreadAbortException) return;

            var exception = context.Exception;

            context.HttpContext.Items.Add("ActionException", exception);

            
            var result = new ErrorResult();
            result.ByError("系统异常，请稍后再试！");

            if (exception is ResultException resultException)
            {
                result.RetCode = resultException.Result.RetCode;
                result.Message = resultException.Result.Message;
            }
            if (!_env.IsProduction())
            {
                result.Error[nameof(Exception.Message)] = exception.Message;
                result.Error[nameof(Exception.StackTrace)] = exception.StackTrace;
            }

            if (context.Result == null)
                context.Result = new ObjectResult(result);

            context.ExceptionHandled = true;
        }

        private string GetRequestBody(HttpRequest request)
        {
            try
            {
                string requestBody;
                var encoding = SelectCharacterEncoding(request.ContentType);
                request.Body.Seek(0, SeekOrigin.Begin);
                using (var reader = new StreamReader(request.Body, encoding, false, 4096, true))
                {
                    requestBody = reader.ReadToEnd();
                }
                return requestBody;
            }
            catch (Exception)
            {
                return "获取RequestBody出错";
            }
        }

        private Encoding SelectCharacterEncoding(string contentType)
        {
            var mediaType = contentType == null ? new MediaType() : new MediaType(contentType);

            return mediaType.Encoding ?? Encoding.ASCII;
        }
    }
}

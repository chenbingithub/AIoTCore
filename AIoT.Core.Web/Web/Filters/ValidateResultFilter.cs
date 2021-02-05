using AIoT.Core.Dto;
using AIoT.Core.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AIoT.Core.Web
{
    /// <summary>
    /// 参数验证失败返回Result
    /// </summary>
    public class ValidateResultFilter : ActionFilterAttribute
    {
        /// <inheritdoc />
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                //var result = new ErrorResult(new SerializableError(context.ModelState), ResultCode.InvalidData);
                //context.Result = new ObjectResult(result);

                var result = context.ModelState.Keys
                        .SelectMany(key => context.ModelState[key].Errors.Select(x => new ValidationError(key, x.ErrorMessage)))
                        .ToList();

                Result<IEnumerable<ValidationError>> apiResult = new Result<IEnumerable<ValidationError>>();

                apiResult.Data = result;
                apiResult.RetCode = ResultCodeEnum.Fail;
                apiResult.Message = "参数校验失败";
                context.Result = new ObjectResult(apiResult);
            }
        }

         /// <summary>
        /// 
        /// </summary>
        public class ValidationError
        {
            /// <summary>
            /// 
            /// </summary>
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string Field { get; }
            /// <summary>
            /// 
            /// </summary>
            public string Message { get; }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="field"></param>
            /// <param name="message"></param>
            public ValidationError(string field, string message)
            {
                Field = field != string.Empty ? field : null;
                Message = message;
            }
        }
    }
}

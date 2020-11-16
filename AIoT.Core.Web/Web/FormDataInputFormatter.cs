using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace AIoT.Core.Web
{
    /// <summary>
    /// 解析 FormData 参数
    /// </summary>
    public class FormDataInputFormatter : InputFormatter
    {
        /// <summary>
        /// 解析 FormData 参数
        /// </summary>
        public FormDataInputFormatter()
        {
            SupportedMediaTypes.Add("multipart/form-data");
            SupportedMediaTypes.Add("application/x-www-form-urlencoded");
        }

        /// <inheritdoc />
        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
        {
            var type = context.ModelType;
            var form = await context.HttpContext.Request.ReadFormAsync();
            if (type.IsAssignableFrom(typeof(IFormCollection)))
            {
                return InputFormatterResult.Success(form);
            }
            if (type.IsAssignableFrom(typeof(IFormCollection)))
            {
                return InputFormatterResult.Success(form.Files);
            }
            if (type.IsAssignableFrom(typeof(List<IFormFile>)))
            {
                return InputFormatterResult.Success(form.Files.ToList());
            }
            if (type.IsAssignableFrom(typeof(IFormFile[])))
            {
                return InputFormatterResult.Success(form.Files.ToArray());
            }
            if (type.IsAssignableFrom(typeof(IFormFile)))
            {
                var file = form.Files.GetFile(context.ModelName);
                if (file == null)
                {
                    file = form.Files.LastOrDefault();
                }
                if (file == null && !context.TreatEmptyInputAsDefaultValue)
                {
                    return InputFormatterResult.NoValue();
                }
                return InputFormatterResult.Success(file);
            }
            
            return InputFormatterResult.Failure();
        }

    }
}
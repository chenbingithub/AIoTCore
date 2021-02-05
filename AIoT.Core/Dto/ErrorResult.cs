using System.Collections.Generic;
using Newtonsoft.Json;

namespace AIoT.Core.Dto
{
    /// <summary>
    /// 异常返回结果
    /// </summary>
    public class ErrorResult : Result
    {
        /// <inheritdoc />
        public ErrorResult() : base(ResultCode.Fail)
        {
            Error = new Dictionary<string, object>();
        }

        /// <inheritdoc />
        public ErrorResult(IDictionary<string, object> error, ResultCode code, string message = null)
        {
            RetCode = code;
            Message = message ?? JsonConvert.SerializeObject(error);
            Error = error;
        }

        /// <summary>
        /// 错误信息
        /// </summary>
        public IDictionary<string, object> Error { get; set; }
    }
}

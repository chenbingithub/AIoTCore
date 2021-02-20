using System;

namespace AIoT.Core.Dto
{
    /// <summary>
    /// 返回指定的 Result 异常信息
    /// </summary>
    public class ResultException : Exception
    {
        /// <inheritdoc cref="ResultException"/>
        public ResultException(ResultCode code, string message = null)
        {
            Result = new Result(code, message);
        }

        /// <inheritdoc cref="ResultException"/>
        public ResultException(IResult result) : base(result.Message)
        {
            Result = result;
        }

        public IResult Result { get; }
    }
}

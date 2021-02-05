using System;
using System.Collections.Generic;

namespace AIoT.Core.Dto
{
    /// <summary>
    /// 返回结果集
    /// </summary>
    public class ListResult<TData> : Result<IList<TData>>
    {
        /// <summary>
        /// 返回结果集
        /// </summary>
        public ListResult()
        {
            Data = new List<TData>();
        }

        /// <summary>
        /// 返回结果集
        /// </summary>
        public ListResult(IList<TData> data)
            : base(data)
        {
        }

        /// <summary>
        /// 返回结果集
        /// </summary>
        /// <param name="code">状态码</param>
        /// <param name="message">提示信息</param>
        public ListResult(ResultCode code, string message = null)
            : base(code, message)
        {
            Data = new List<TData>();
        }


        #region 静态函数

        /// <summary>
        /// <see cref="ResultCode"/> 隐式转换为 <see cref="ListResult{TData}"/>
        /// </summary>
        public static implicit operator ListResult<TData>(ResultCode code) => ListResult.FromCode<TData>(code);

        /// <summary>
        /// <see cref="ValueTuple"/> 隐式转换为 <see cref="ListResult{TData}"/>
        /// </summary>
        public static implicit operator ListResult<TData>((ResultCode Code, string Message) tuple)
            => ListResult.FromCode<TData>(tuple.Code, tuple.Message);

        /// <summary>
        /// <see cref="ListResult{TData}"/> 隐式转换为 <see cref="ResultCode"/>
        /// </summary>
        public static implicit operator ResultCode(ListResult<TData> result) => result.RetCode;

        /// <summary>
        /// <see cref="List{TData}"/> 隐式转换为 <see cref="ListResult{TData}"/>
        /// </summary>
        public static implicit operator ListResult<TData>(List<TData> data) => ListResult.FromData(data);

        /// <summary>
        /// [] 隐式转换为 <see cref="ListResult{TData}"/>
        /// </summary>
        public static implicit operator ListResult<TData>(TData[] data) => ListResult.FromData(data);

        #endregion
    }

    /// <summary>
    /// 返回结果集(带合计行)
    /// </summary>
    public class TotalListResult<TData> : ListResult<TData>
    {
        /// <summary>
        /// 合计行
        /// </summary>
        public TData Total { get; set; }

        /// <summary>
        /// 返回结果集
        /// </summary>
        public TotalListResult(IList<TData> data, TData total)
            : base(data)
        {
            Total = total;
        }

        /// <summary>
        /// 返回结果集
        /// </summary>
        /// <param name="code">状态码</param>
        /// <param name="message">提示信息</param>
        public TotalListResult(ResultCode code, string message = null)
            : base(code, message)
        {
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class ListResult
    {
        #region 静态函数

        /// <summary>
        /// 返回指定 Code
        /// </summary>
        public static ListResult<T> FromCode<T>(ResultCode code, string message = null)
        {
            return new ListResult<T>(code, message);
        }

        /// <summary>
        /// 返回异常信息
        /// </summary>
        public static ListResult<T> FromError<T>(string message)
        {
            return new ListResult<T>(ResultCode.Fail, message);
        }

        /// <summary>
        /// 返回异常信息
        /// </summary>
        public static ListResult<T> FromError<T>(string message, ResultCode code)
        {
            return new ListResult<T>(code, message);
        }

        /// <summary>
        /// 返回新结果
        /// </summary>
        public static ListResult<T> FromResult<T>(Result result)
        {
            return new ListResult<T>(result.RetCode, result.Message);
        }

        /// <summary>
        /// 返回新结果
        /// </summary>
        public static ListResult<T> FromResult<T>(Result result, IList<T> data)
        {
            return new ListResult<T>(result.RetCode, result.Message) { Data = data };
        }

        /// <summary>
        /// 返回新结果
        /// </summary>
        public static ListResult<T> FromResult<T>(Result<IList<T>> result)
        {

            return new ListResult<T>(result.RetCode, result.Message) { Data = result.Data };
        }

        /// <summary>
        /// 返回数据
        /// </summary>
        public static ListResult<T> FromData<T>(IList<T> data)
        {
            return new ListResult<T>(data);
        }

        /// <summary>
        /// 返回成功
        /// </summary>
        public static ListResult<T> Ok<T>(IList<T> data)
        {
            return FromData(data);
        }

        #endregion
    }
}

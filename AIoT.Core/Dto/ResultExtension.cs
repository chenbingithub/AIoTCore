using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AIoT.Core.Dto
{
    /// <summary>
    /// 扩展 <see cref="IResult"/> 类
    /// </summary>
    public static class ResultExtension
    {
        /// <summary>
        /// 是否为成功状态
        /// </summary>
        public static bool IsOk(this IResult result)
        {
            return result?.RetCode == ResultCode.Ok;
        }

        /// <summary>
        /// 返回指定 Code
        /// </summary>
        public static TResult ByCode<TResult>(this TResult result, ResultCode code, string message = null)
            where TResult : IResult
        {
            result.RetCode = code;
            if (message != null)
                result.Message = message;
            return result;
        }

        /// <summary>
        /// 返回异常信息
        /// </summary>
        public static TResult ByError<TResult>(this TResult result, string message)
            where TResult : IResult
        {
            result.RetCode = ResultCode.Fail;
            if (message != null)
                result.Message = message;
            return result;
        }

        /// <summary>
        /// 返回异常信息
        /// </summary>
        public static TResult ByError<TResult>(this TResult result, string message, ResultCode code)
            where TResult : IResult
        {
            result.RetCode = code;
            if (message != null)
                result.Message = message;
            return result;
        }

        


        /// <summary>
        /// 返回新结果
        /// </summary>
        public static TResult ByResult<TResult>(this TResult result, IResult otherResult)
            where TResult : IResult
        {
            result.RetCode = otherResult.RetCode;
            result.Message = otherResult.Message;
            return result;
        }

        /// <summary>
        /// 返回新结果
        /// </summary>
        public static TResult ByResultData<TResult, TData>(this TResult result, IResult<TData> otherResult)
            where TResult : IResult<TData>
        {
            result.RetCode = otherResult.RetCode;
            result.Message = otherResult.Message;
            result.Data = otherResult.Data;
            return result;
        }

        /// <summary>
        /// 返回数据
        /// </summary>
        public static TResult ByData<TResult, TData>(this TResult result, TData data)
            where TResult : IResult<TData>
        {
            result.RetCode = ResultCode.Ok;
            result.Message = null;
            result.Data = data;
            return result;
        }

        /// <summary>
        /// 返回成功
        /// </summary>
        public static TResult ByOk<TResult>(this TResult result, string message = null)
            where TResult : IResult
        {
            result.RetCode = ResultCode.Ok;
            result.Message = message;
            return result;
        }

        /// <summary>
        /// 返回数据
        /// </summary>
        public static TResult ByOk<TResult, TData>(this TResult result, TData data)
            where TResult : IResult<TData>
        {
            result.RetCode = ResultCode.Ok;
            result.Message = null;
            result.Data = data;
            return result;
        }


        /// <summary>
        /// 返回结果数据
        /// </summary>
        public static async Task<Dto.Result> ToResultAsync(this Task task)
        {
            await task;
            return Dto.Result.Ok();
        }

        /// <summary>
        /// 返回结果数据
        /// </summary>
        /// <param name="data"></param>
        /// <param name="noRecode">如果 <see cref="Result{T}.Data"/> 为 Null，则状态码为 <see cref="ResultCode.NoRecord"/></param>
        public static async Task<Result<T>> ToResultAsync<T>(this Task<T> data, bool noRecode = false)
        {
            var rs = Dto.Result.FromData(await data);
            if (noRecode && rs.Data == null)
                rs.RetCode = ResultCode.NoRecord;

            return rs;
        }

        /// <summary>
        /// 返回列表数据
        /// </summary>
        public static async Task<ListResult<TElement>> ToListResultAsync<TCollection, TElement>(
            this Task<TCollection> source)
            where TCollection : IEnumerable<TElement>
        {
            var result = await source;
            return new ListResult<TElement>(result.ToList());
        }
    }
}

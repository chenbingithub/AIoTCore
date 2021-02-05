using System;

namespace AIoT.Core.Dto
{
    /// <summary>
    /// 返回结果
    /// </summary>
    //[DataContract]
    public class Result : IResult
    {
        private string _message;

        /// <summary>
        /// 返回结果
        /// </summary>
        public Result()
        {

        }

        /// <summary>
        /// 返回结果
        /// </summary>
        /// <param name="code">状态码</param>
        /// <param name="message">提示信息</param>
        public Result(int code, string message)
        {
            RetCode = code;
            Message = message;
        }

        /// <summary>
        /// 返回结果
        /// </summary>
        /// <param name="code">状态码</param>
        /// <param name="message">提示信息</param>
        public Result(ResultCode code, string message = null)
        {
            RetCode = code;
            Message = message;
        }

        /// <summary>
        /// 结果状态码
        /// </summary>
        //[DataMember(Order = 1)]
        public ResultCode RetCode { get; set; }

        /// <summary>
        /// 提示信息
        /// </summary>
        /// <example>操作成功</example>
        //[DataMember(Order = 2)]
        public string Message
        {
            get => _message ?? RetCode.DisplayName();
            set => _message = value;
        }

        /// <summary>
        /// 是否成功
        /// </summary>
        //[DataMember(Order = 3)]
        public bool Success => RetCode == ResultCode.Ok;

        #region 静态函数

        /// <summary>
        /// 返回指定 Code
        /// </summary>
        public static Result FromCode(ResultCode code, string message = null)
        {
            return new Result(code, message);
        }

        /// <summary>
        /// 返回指定 Code
        /// </summary>
        public static Result<T> FromCode<T>(ResultCode code, string message = null)
        {
            return new Result<T>(code, message);
        }

        /// <summary>
        /// 返回指定 Code
        /// </summary>
        public static SubCodeResult<T> FromSubCode<T>(T subCode, string subMessage = null, ResultCode code = default, string message = null)
            where T : struct, Enum
        {
            return new SubCodeResult<T>(code, subCode, subMessage, message);
        }

        /// <summary>
        /// 返回指定 Code
        /// </summary>
        public static SubCodeResult<TSubCode, TData> FromSubCode<TSubCode, TData>(TSubCode subCode, string subMessage = null, ResultCode code = default, string message = null)
            where TSubCode : struct, Enum
        {
            return new SubCodeResult<TSubCode, TData>(code, subCode, subMessage, message);
        }

        /// <summary>
        /// 返回异常信息
        /// </summary>
        public static Result FromError(string message)
        {
            return new Result(ResultCode.Fail, message);
        }

        /// <summary>
        /// 返回异常信息
        /// </summary>
        public static Result FromError(string message, ResultCode code)
        {
            return new Result(code, message);
        }

        /// <summary>
        /// 返回异常信息
        /// </summary>
        public static Result<T> FromError<T>(string message)
        {
            return new Result<T>(ResultCode.Fail, message);
        }

        /// <summary>
        /// 返回异常信息
        /// </summary>
        public static Result<T> FromError<T>(string message, ResultCode code)
        {
            return new Result<T>(code, message);
        }

        /// <summary>
        /// 返回新结果
        /// </summary>
        public static Result FromResult(IResult result)
        {
            return new Result(result.RetCode, result.Message);
        }

        /// <summary>
        /// 返回新结果
        /// </summary>
        public static Result<T> FromResult<T>(IResult result)
        {
            return new Result<T>(result.RetCode, result.Message);
        }

        /// <summary>
        /// 返回新结果
        /// </summary>
        public static Result<T> FromResult<T>(IResult result, T data)
        {
            return new Result<T>(result.RetCode, result.Message) { Data = data };
        }

        /// <summary>
        /// 返回新结果
        /// </summary>
        public static Result<T> FromResult<T>(IResult<T> result)
        {

            return new Result<T>(result.RetCode, result.Message) { Data = result.Data };
        }

        /// <summary>
        /// 返回数据
        /// </summary>
        public static Result<T> FromData<T>(T data)
        {
            return new Result<T>(data);
        }

        /// <summary>
        /// 返回成功
        /// </summary>
        public static Result Ok(string message = null)
        {
            return FromCode(ResultCode.Ok, message);
        }

        /// <summary>
        /// 返回成功
        /// </summary>
        public static Result<T> Ok<T>(T data)
        {
            return FromData(data);
        }

        /// <summary>
        /// <see cref="ResultCode"/> 隐式转换为 <see cref="Result"/>
        /// </summary>
        public static implicit operator Result(ResultCode code) => FromCode(code);

        /// <summary>
        /// <see cref="ValueTuple{ResultCode, Message}"/> 隐式转换为 <see cref="Result"/>
        /// </summary>
        public static implicit operator Result((ResultCode Code, string Message) tuple)
            => FromCode(tuple.Code, tuple.Message);

        /// <summary>
        /// <see cref="Result"/> 隐式转换为 <see cref="ResultCode"/>
        /// </summary>
        public static implicit operator ResultCode(Result result) => result.RetCode;

        /// <summary>
        /// 解构 <see cref="Result"/> 为 <see cref="ValueTuple{ResultCode, Message}"/>
        /// </summary>
        public void Deconstruct(out ResultCode code, out string message)
        {
            code = RetCode;
            message = Message;
        }

        #endregion
    }

    /// <summary>
    /// 返回结果
    /// </summary>
    //[DataContract]
    public class Result<TData> : Result, IResult<TData>
    {
        /// <summary>
        /// 返回结果
        /// </summary>
        public Result()
        {

        }

        /// <summary>
        /// 返回结果
        /// </summary>
        public Result(TData data)
            : base(ResultCode.Ok)
        {
            Data = data;
        }

        /// <summary>
        /// 返回结果
        /// </summary>
        /// <param name="code">状态码</param>
        /// <param name="message">提示信息</param>
        public Result(ResultCode code, string message = null)
            : base(code, message)
        {

        }

        /// <summary>
        /// 返回结果数据
        /// </summary>
        //[DataMember(Order = 4)]
        public TData Data { get; set; }

        #region 静态函数

        /// <summary>
        /// <see cref="ResultCode"/> 隐式转换为 <see cref="Result{TData}"/>
        /// </summary>
        public static implicit operator Result<TData>(ResultCode code) => FromCode<TData>(code);

        /// <summary>
        /// <see cref="ValueTuple{ResultCode, Message}"/> 隐式转换为 <see cref="Result{TData}"/>
        /// </summary>
        public static implicit operator Result<TData>((ResultCode Code, string Message) tuple)
            => FromCode<TData>(tuple.Code, tuple.Message);

        /// <summary>
        /// <see cref="Result{TData}"/> 隐式转换为 <see cref="ResultCode"/>
        /// </summary>
        public static implicit operator ResultCode(Result<TData> result) => result.RetCode;

        /// <summary>
        /// <typeparamref name="TData"/> 隐式转换为 <see cref="Result{TData}"/>
        /// </summary>
        public static implicit operator Result<TData>(TData data) => FromData(data);

        /// <summary>
        /// <see cref="Result{TData}"/> 隐式转换为 <typeparamref name="TData"/>
        /// </summary>
        public static implicit operator TData(Result<TData> result) => result.Data;

        /// <summary>
        /// 解构 <see cref="Result"/> 为 <see cref="ValueTuple{ResultCode, Message, TData}"/>
        /// </summary>
        public void Deconstruct(out ResultCode code, out string message, out TData data)
        {
            code = RetCode;
            message = Message;
            data = Data;
        }

        #endregion
    }
}

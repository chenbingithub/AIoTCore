using System;
using System.Collections.Generic;
using System.Text;

namespace AIoT.Core.Dto
{
    /// <summary>
    /// 返回结果
    /// </summary>
    public interface ISubCodeResult<TSubCode> : IResult
        where TSubCode : struct, Enum
    {
        /// <summary>
        /// 子状态码
        /// </summary>
        TSubCode SubCode { get; set; }

        /// <summary>
        /// 子提示信息
        /// </summary>
        string SubMessage { get; set; }
    }

    /// <summary>
    /// 返回结果
    /// </summary>
    public interface ISubCodeResult<TSubCode, TData> : IResult<TData>, ISubCodeResult<TSubCode>
        where TSubCode : struct, Enum
    {
    }
}

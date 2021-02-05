using System;
using System.Collections.Generic;
using System.Text;
using AIoT.Core.Enums;
using AIoT.Core.Extensions;

namespace AIoT.Core.Dto
{
    /// <summary>
    /// 带 SubCode 的返回结果
    /// </summary>
    public class SubCodeResult<TSubCode> : Result, ISubCodeResult<TSubCode>
        where TSubCode : struct, Enum
    {
        #region 构造函数、私有字段

        private string _subMessage;

        /// <summary>
        /// 返回结果
        /// </summary>
        public SubCodeResult()
        {

        }

        /// <summary>
        /// 返回结果
        /// </summary>
        /// <param name="code">状态码</param>
        /// <param name="subCode">子状态码</param>
        /// <param name="subMessage">子提示信息</param>
        /// <param name="message">提示信息</param>
        public SubCodeResult(ResultCode code, TSubCode subCode, string subMessage = null, string message = null)
            : base(code, message)
        {
            SubCode = subCode;
            SubMessage = subMessage;
        }

        #endregion

        /// <summary>
        /// 子状态码
        /// </summary>
        public TSubCode SubCode { get; set; }

        /// <summary>
        /// 子提示信息
        /// </summary>
        public string SubMessage
        {
            get => _subMessage ?? SubCode.DisplayName();
            set => _subMessage = value;
        }
    }

    /// <summary>
    /// 带 SubCode 的返回结果
    /// </summary>
    public class SubCodeResult<TSubCode, TData> : Result<TData>, ISubCodeResult<TSubCode, TData>
        where TSubCode : struct, Enum
    {
        #region 构造函数、私有字段

        private string _subMessage;

        /// <summary>
        /// 返回结果
        /// </summary>
        public SubCodeResult()
        {

        }
        /// <summary>
        /// 返回结果
        /// </summary>
        public SubCodeResult(TData data, TSubCode subCode = default, string subMessage = null)
            : base(data)
        {
            SubCode = subCode;
            SubMessage = subMessage;
        }

        /// <summary>
        /// 返回结果
        /// </summary>
        /// <param name="code">状态码</param>
        /// <param name="subCode">子状态码</param>
        /// <param name="subMessage">子提示信息</param>
        /// <param name="message">提示信息</param>
        public SubCodeResult(ResultCode code, TSubCode subCode, string subMessage = null, string message = null)
            : base(code, message)
        {
            SubCode = subCode;
            SubMessage = subMessage;
        }

        #endregion

        /// <summary>
        /// 子状态码
        /// </summary>
        public TSubCode SubCode { get; set; }

        /// <summary>
        /// 子提示信息
        /// </summary>
        public string SubMessage
        {
            get => _subMessage ?? SubCode.DisplayName();
            set => _subMessage = value;
        }
    }
}

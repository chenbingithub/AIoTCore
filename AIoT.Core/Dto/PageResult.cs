using System;
using System.Collections.Generic;

namespace AIoT.Core.Dto
{
    /// <summary>
    /// 分页返回结果
    /// </summary>
    public class PageResult<TData> : ListResult<TData>, IPageResult
    {
        /// <summary>
        /// 分页返回结果
        /// </summary>
        public PageResult()
        {

        }

        /// <summary>
        /// 分页返回结果
        /// </summary>
        public PageResult(IList<TData> data, int totalCount, int pageIndex, int pageSize)
            : base(data)
        {
            TotalCount = totalCount;
            PageIndex = pageIndex;
            PageSize = pageSize;
            //Data = data.Skip(pageSize * (pageIndex - 1)).Take(pageSize).ToList();
            Data = data;
        }

        /// <summary>
        /// 分页返回结果
        /// </summary>
        public PageResult(IList<TData> data, int totalCount, IPageInfo pager)
            : this(data, totalCount, pager?.PageIndex ?? 0, pager?.PageSize ?? 0)
        {

        }

        /// <summary>
        /// 分页返回结果
        /// </summary>
        /// <param name="code">状态码</param>
        /// <param name="message">提示信息</param>
        public PageResult(ResultCode code, string message = null)
            : base(code, message)
        {

        }

        /// <summary>
        /// 当前页
        /// </summary>
        /// <example>1</example>
        public int PageIndex { get; set; }

        /// <summary>
        /// 每页大小
        /// </summary>
        /// <example>30</example>
        public int PageSize { get; set; }

        /// <summary>
        /// 总页数
        /// </summary>
        /// <example>5</example>
        public int TotalPage => PageSize > 0 ? (int)Math.Ceiling((decimal)TotalCount / PageSize) : 0;

        /// <summary>
        /// 数据总条数
        /// </summary>
        /// <example>250</example>
        public int TotalCount { get; set; }


        /// <summary>
        /// <see cref="ResultCode"/> 隐式转换为 <see cref="PageResult{TData}"/>
        /// </summary>
        public static implicit operator PageResult<TData>(ResultCode code) => new PageResult<TData>(code);

        /// <summary>
        /// <see cref="ValueTuple{ResultCode, Message}"/> 隐式转换为 <see cref="PageResult{TData}"/>
        /// </summary>
        public static implicit operator PageResult<TData>((ResultCode Code, string Message) tuple)
            => new PageResult<TData>(tuple.Code, tuple.Message);

        /// <summary>
        /// <see cref="PageResult{TData}"/> 隐式转换为 <see cref="ResultCode"/>
        /// </summary>
        public static implicit operator ResultCode(PageResult<TData> result) => result.RetCode;
    }

    /// <summary>
    /// 分页返回结果(带合计行)
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    public class TotalPageResult<TData> : PageResult<TData>
    {
        /// <summary>
        /// 合计行
        /// </summary>
        public TData Total { get; set; }

        /// <summary>
        /// 分页返回结果(带合计行)
        /// </summary>
        public TotalPageResult()
        {
        }

        /// <summary>
        /// 分页返回结果
        /// </summary>
        public TotalPageResult(IList<TData> data, TData total, int totalCount, int pageIndex, int pageSize)
            : base(data, totalCount, pageIndex, pageSize)
        {
            TotalCount = totalCount;
            PageIndex = pageIndex;
            PageSize = pageSize;
            Data = data;
            Total = total;
        }

        /// <summary>
        /// 分页返回结果
        /// </summary>
        public TotalPageResult(PageResult<TData> data, TData total)
            : this(data?.Data, total, data?.TotalCount ?? 0, data?.PageIndex ?? 0, data?.PageSize ?? 0)
        {
        }


        /// <summary>
        /// 分页返回结果
        /// </summary>
        public TotalPageResult(IList<TData> data, TData total, int totalCount, IPageInfo pager)
            : this(data, total, totalCount, pager?.PageIndex ?? 0, pager?.PageSize ?? 0)
        {

        }

        /// <summary>
        /// 分页返回结果
        /// </summary>
        /// <param name="code">状态码</param>
        /// <param name="message">提示信息</param>
        public TotalPageResult(ResultCode code, string message = null)
            : base(code, message)
        {

        }
    }
}

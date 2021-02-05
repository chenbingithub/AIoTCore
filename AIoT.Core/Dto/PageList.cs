using System;
using System.Collections.Generic;
using System.Text;

namespace AIoT.Core.Dto
{
    /// <summary>
    /// 分页数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PageList<T> where T : new()
    {
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
        /// 分页数据
        /// </summary>
        public IList<T> Data { get; set; }
    }
}

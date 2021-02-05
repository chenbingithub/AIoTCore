using System.Collections.Generic;

namespace AIoT.Core.Dto
{
    /// <summary>
    /// 分页排序信息
    /// </summary>
    public class PageSortInfo : PageInfo, IPageSortInfo
    {
        /// <summary>
        /// 分页排序信息
        /// </summary>
        public PageSortInfo()
        {
            SortFields = new List<string>();
        }

        /// <summary>
        /// 分页排序信息
        /// </summary>
        public PageSortInfo(int pageIndex, int pageSize = 10, IEnumerable<string> sort = null)
        {
            PageSize = pageSize;
            PageIndex = pageIndex;

            var sortFields = new List<string>();
            if (sort != null) sortFields.AddRange(sort);

            SortFields = sortFields;
        }

        /// <summary>
        /// 排序字段
        /// </summary>
        /// <example>['SortNo desc', 'CreateTime desc']</example>
        public IList<string> SortFields { get; set; }
    }
}
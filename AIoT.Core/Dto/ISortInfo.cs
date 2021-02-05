using System.Collections.Generic;

namespace AIoT.Core.Dto
{
    /// <summary>
    /// ������Ϣ
    /// </summary>
    public interface ISortInfo
    {
        /// <summary>
        /// �����ֶ�
        /// </summary>
        /// <example>['SortNo desc', 'CreateTime desc']</example>
        IList<string> SortFields { get; set; }
    }
}
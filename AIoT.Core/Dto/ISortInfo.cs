using System.Collections.Generic;

namespace AIoT.Core.Dto
{
    /// <summary>
    /// ≈≈–Ú–≈œ¢
    /// </summary>
    public interface ISortInfo
    {
        /// <summary>
        /// ≈≈–Ú◊÷∂Œ
        /// </summary>
        /// <example>['SortNo desc', 'CreateTime desc']</example>
        IList<string> SortFields { get; set; }
    }
}
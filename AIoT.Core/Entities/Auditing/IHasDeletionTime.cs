using System;

namespace AIoT.Core.Entities.Auditing
{
    /// <summary>
    /// 标识软删除时间
    /// </summary>
    public interface IHasDeletionTime : ISoftDelete
    {
        /// <summary>
        /// 删除时间
        /// </summary>
        DateTime? DeletionTime { get; set; }
    }
}
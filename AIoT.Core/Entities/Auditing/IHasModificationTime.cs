using System;

namespace AIoT.Core.Entities.Auditing
{
    /// <summary>
    /// 标识修改时间
    /// </summary>
    public interface IHasModificationTime
    {
        /// <summary>
        /// 最后修改时间
        /// </summary>
        DateTime? LastModificationTime { get; set; }
    }
}
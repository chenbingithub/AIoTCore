using System;

namespace AIoT.Core.Entities.Auditing
{
    /// <summary>
    /// 标识创建时间
    /// </summary>
    public interface IHasCreationTime
    {
        /// <summary>
        /// 创建时间
        /// </summary>
        DateTime CreationTime { get; set; }
    }
}
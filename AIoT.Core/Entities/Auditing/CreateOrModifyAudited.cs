using System;

namespace AIoT.Core.Entities.Auditing
{
    /// <summary>
    /// 标识最后修改时间
    /// </summary>
    public abstract class CreateOrModifyAudited<TUserId> : Entity, IModificationAudited<TUserId>
    {
        /// <summary>
        /// 创建或修改时间
        /// </summary>
        public DateTime? LastModificationTime { get; set; }

        /// <summary>
        /// 创建或修改人
        /// </summary>
        public TUserId LastModifierUserId { get; set; }
    }

    /// <summary>
    /// 标识最后修改时间
    /// </summary>
    public abstract class CreateOrModifyAudited<TKey, TUserId> : Entity<TKey>, IModificationAudited<TUserId>
    {
        /// <summary>
        /// 创建或修改时间
        /// </summary>
        public DateTime? LastModificationTime { get; set; }

        /// <summary>
        /// 创建或修改人
        /// </summary>
        public TUserId LastModifierUserId { get; set; }
    }
}

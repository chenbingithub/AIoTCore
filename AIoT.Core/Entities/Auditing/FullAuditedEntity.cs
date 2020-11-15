using System;

namespace AIoT.Core.Entities.Auditing
{
    /// <inheritdoc cref="IDeletionAudited{TUserId}" />
    public abstract class FullAuditedEntity<TUserId> : ModificationAudited<TUserId>, IDeletionAudited<TUserId>
    {
        /// <summary>
        /// 标记是否已删除
        /// </summary>
        public virtual bool IsDeleted { get; set; }

        /// <summary>
        /// 删除时间
        /// </summary>
        public virtual DateTime? DeletionTime { get; set; }

        /// <summary>
        /// 删除者
        /// </summary>
        public virtual TUserId DeleterUserId { get; set; }
    }

    /// <inheritdoc cref="IDeletionAudited{TUserId}" />
    public abstract class FullAuditedEntity<TKey, TUserId> : ModificationAudited<TKey, TUserId>, IDeletionAudited<TUserId>
    {
        /// <summary>
        /// 标记是否已删除
        /// </summary>
        public virtual bool IsDeleted { get; set; }

        /// <summary>
        /// 删除时间
        /// </summary>
        public virtual DateTime? DeletionTime { get; set; }

        /// <summary>
        /// 删除者
        /// </summary>
        public virtual TUserId DeleterUserId { get; set; }
    }
}

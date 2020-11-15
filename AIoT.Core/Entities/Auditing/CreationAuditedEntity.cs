using System;

namespace AIoT.Core.Entities.Auditing
{
    /// <inheritdoc cref="ICreationAudited{TUserId}" />
    public abstract class CreationAuditedEntity<TUserId> : Entity, ICreationAudited<TUserId>
    {
        /// <summary>
        /// 创建时间
        /// </summary>
        public virtual DateTime CreationTime { get; set; }

        /// <summary>
        /// 创建者
        /// </summary>
        public virtual TUserId CreatorUserId { get; set; }
    }

    /// <inheritdoc cref="ICreationAudited{TUserId}" />
    public abstract class CreationAuditedEntity<TKey, TUserId> : Entity<TKey>, ICreationAudited<TUserId>
    {
        /// <summary>
        /// 创建时间
        /// </summary>
        public virtual DateTime CreationTime { get; set; }

        /// <summary>
        /// 创建者
        /// </summary>
        public virtual TUserId CreatorUserId { get; set; }
    }
}

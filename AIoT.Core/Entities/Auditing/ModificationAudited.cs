using System;

namespace AIoT.Core.Entities.Auditing
{
    /// <inheritdoc cref="IModificationAudited{TUserId}"/>
    public abstract class ModificationAudited<TUserId> : CreationAuditedEntity<TUserId>, IModificationAudited<TUserId>
    {
        /// <inheritdoc />
        public DateTime? LastModificationTime { get; set; }

        /// <inheritdoc />
        public TUserId LastModifierUserId { get; set; }
    }

    /// <inheritdoc cref="IModificationAudited{TUserId}"/>
    public abstract class ModificationAudited<TKey, TUserId> : CreationAuditedEntity<TKey, TUserId>, IModificationAudited<TUserId>
    {
        /// <inheritdoc />
        public DateTime? LastModificationTime { get; set; }

        /// <inheritdoc />
        public TUserId LastModifierUserId { get; set; }
    }
}

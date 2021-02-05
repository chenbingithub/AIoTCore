using System;
using System.ComponentModel.DataAnnotations;

namespace AIoT.Core.Entities.Auditing
{
    /// <inheritdoc cref="IDeletionAudited{TUserId}" />
    public abstract class FullAuditedEntityName<TUserId> : ModificationAudited<TUserId>, IDeletionAudited<TUserId>
    , ICreationAuditedName, IDeletionAuditedName, IModificationAuditedName
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
        [StringLength(50)]
        public string LastModifierUserName { get; set; }
        [StringLength(50)]
        public string DeleterUserName { get; set; }
        [StringLength(50)]
        public string CreatorUserName { get; set; }
    }

    /// <inheritdoc cref="IDeletionAudited{TUserId}" />
    public abstract class FullAuditedEntityName<TKey, TUserId> : ModificationAudited<TKey, TUserId>, IDeletionAudited<TUserId>,
         ICreationAuditedName, IDeletionAuditedName, IModificationAuditedName
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
        [StringLength(50)]
        public string LastModifierUserName { get; set; }
        [StringLength(50)]
        public string DeleterUserName { get; set; }
        [StringLength(50)]
        public string CreatorUserName { get; set; }
    }
}

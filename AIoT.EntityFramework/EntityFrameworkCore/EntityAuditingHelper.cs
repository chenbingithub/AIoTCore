using System;
using AIoT.Core.Entities.Auditing;

namespace AIoT.EntityFramework.EntityFrameworkCore
{
    /// <summary>
    /// 设置审计字段参数
    /// UserId 只支持 string, int, int?, long, long?, Guid, Guid?
    /// </summary>
    public class EntityAuditingHelper
    {
        /// <summary>
        /// 设置添加审计数据
        /// </summary>
        public static void SetCreationAuditProperties(object entity, string userId,string username="")
        {
            if (entity is IHasCreationTime creationTimeEntity &&
                creationTimeEntity.CreationTime == default)
            {
                creationTimeEntity.CreationTime = DateTime.Now;
            }
            if (entity is ICreationAuditedName creatorName)
            {
                creatorName.CreatorUserName = username;
            }
            if (userId != null)
            {
                SetCreatorUserId(entity, userId);
            }

            
        }

        /// <summary>
        /// 设置修改审计数据
        /// </summary>
        public static void SetModificationAuditProperties(object entity, string userId, string username = "")
        {
            if (entity is IHasModificationTime modificationTime)
            {
                modificationTime.LastModificationTime = DateTime.Now;
            }
            if (entity is IModificationAuditedName lastModifierName)
            {
                lastModifierName.LastModifierUserName = username;
            }
            if (userId != null)
            {
                SetLastModifierUserId(entity, userId);
            }
        }

        /// <summary>
        /// 设置删除审计数据
        /// </summary>
        public static void SetDeletionAuditProperties(object entity, string userId, string username = "")
        {
            if (entity is IHasDeletionTime deletionTime)
            {
                deletionTime.DeletionTime = DateTime.Now;
            }
            if (entity is IDeletionAuditedName deletionName)
            {
                deletionName.DeleterUserName = username;
            }

            if (userId != null)
            {
                SetDeleterUserId(entity, userId);
            }
        }

        /// <summary>
        /// 设置 <see cref="ICreationAudited{TUserId}.CreatorUserId"/>
        /// UserId 只支持 string, int, int?, long, long?, Guid, Guid?
        /// </summary>
        public static void SetCreatorUserId(object entity, string userId)
        {
            if (userId == null) return;

            switch (entity)
            {
                case ICreationAudited<string> strAudit:
                    strAudit.CreatorUserId = userId;
                    break;
                case ICreationAudited<int> intAudit:
                    if (intAudit.CreatorUserId != default && int.TryParse(userId, out var intId))
                    {
                        intAudit.CreatorUserId = intId;
                    }
                    break;
                case ICreationAudited<int?> nullIntAudit:
                    if (nullIntAudit.CreatorUserId != default && int.TryParse(userId, out intId))
                    {
                        nullIntAudit.CreatorUserId = intId;
                    }
                    break;
                case ICreationAudited<long> longAudit:
                    if (longAudit.CreatorUserId != default && long.TryParse(userId, out var longId))
                    {
                        longAudit.CreatorUserId = longId;
                    }
                    break;
                case ICreationAudited<long?> nullLongAudit:
                    if (nullLongAudit.CreatorUserId != default && long.TryParse(userId, out longId))
                    {
                        nullLongAudit.CreatorUserId = longId;
                    }
                    break;
                case ICreationAudited<Guid> guidAudit:
                    if (guidAudit.CreatorUserId != default && Guid.TryParse(userId, out var guidId))
                    {
                        guidAudit.CreatorUserId = guidId;
                    }
                    break;
                case ICreationAudited<Guid?> nullGuidAudit:
                    if (nullGuidAudit.CreatorUserId != default && Guid.TryParse(userId, out guidId))
                    {
                        nullGuidAudit.CreatorUserId = guidId;
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 设置 <see cref="IModificationAudited{TUserId}.LastModifierUserId"/>
        /// UserId 只支持 string, int, int?, long, long?, Guid, Guid?
        /// </summary>
        public static void SetLastModifierUserId(object entity, string userId)
        {
            if (userId == null) return;

            switch (entity)
            {
                case IModificationAudited<string> strAudit:
                    strAudit.LastModifierUserId = userId;
                    break;
                case IModificationAudited<int> intAudit:
                    if (int.TryParse(userId, out var intId))
                    {
                        intAudit.LastModifierUserId = intId;
                    }
                    break;
                case IModificationAudited<int?> nullIntAudit:
                    if (int.TryParse(userId, out intId))
                    {
                        nullIntAudit.LastModifierUserId = intId;
                    }
                    break;
                case IModificationAudited<long> longAudit:
                    if (long.TryParse(userId, out var longId))
                    {
                        longAudit.LastModifierUserId = longId;
                    }
                    break;
                case IModificationAudited<long?> nullLongAudit:
                    if (long.TryParse(userId, out longId))
                    {
                        nullLongAudit.LastModifierUserId = longId;
                    }
                    break;
                case IModificationAudited<Guid> guidAudit:
                    if (Guid.TryParse(userId, out var guidId))
                    {
                        guidAudit.LastModifierUserId = guidId;
                    }
                    break;
                case IModificationAudited<Guid?> nullGuidAudit:
                    if (Guid.TryParse(userId, out guidId))
                    {
                        nullGuidAudit.LastModifierUserId = guidId;
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 设置 <see cref="IDeletionAudited{TUserId}.DeleterUserId"/>
        /// UserId 只支持 string, int, int?, long, long?, Guid, Guid?
        /// </summary>
        public static void SetDeleterUserId(object entity, string userId)
        {
            if (userId == null) return;

            switch (entity)
            {
                case IDeletionAudited<string> strAudit:
                    strAudit.DeleterUserId = userId;
                    break;
                case IDeletionAudited<int> intAudit:
                    if (int.TryParse(userId, out var intId))
                    {
                        intAudit.DeleterUserId = intId;
                    }
                    break;
                case IDeletionAudited<int?> nullIntAudit:
                    if (int.TryParse(userId, out intId))
                    {
                        nullIntAudit.DeleterUserId = intId;
                    }
                    break;
                case IDeletionAudited<long> longAudit:
                    if (long.TryParse(userId, out var longId))
                    {
                        longAudit.DeleterUserId = longId;
                    }
                    break;
                case IDeletionAudited<long?> nullLongAudit:
                    if (long.TryParse(userId, out longId))
                    {
                        nullLongAudit.DeleterUserId = longId;
                    }
                    break;
                case IDeletionAudited<Guid> guidAudit:
                    if (Guid.TryParse(userId, out var guidId))
                    {
                        guidAudit.DeleterUserId = guidId;
                    }
                    break;
                case IDeletionAudited<Guid?> nullGuidAudit:
                    if (Guid.TryParse(userId, out guidId))
                    {
                        nullGuidAudit.DeleterUserId = guidId;
                    }
                    break;
                default:
                    break;
            }
        }
    }
}

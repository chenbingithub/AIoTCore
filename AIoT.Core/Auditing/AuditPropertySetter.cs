using System;
using AIoT.Core.Entities.Auditing;
using AIoT.Core.Runtime;
using AIoT.Core.Timing;
using Volo.Abp.DependencyInjection;


namespace AIoT.Core.Auditing
{
    public class AuditPropertySetter : IAuditPropertySetter, ITransientDependency
    {
        private ICurrentUser CurrentUser { get; }
        private IClock Clock { get; }

        public AuditPropertySetter(
            ICurrentUser currentUser,
            IClock clock)
        {
            CurrentUser = currentUser;
            Clock = clock;
        }

        public void SetCreationProperties(object targetObject)
        {
            SetCreationTime(targetObject);
            SetCreatorId(targetObject);
            if (targetObject is ICreationAuditedName userNameEntity)
            {
                userNameEntity.CreatorUserName = GetUserName();
            }
        }

        public void SetModificationProperties(object targetObject)
        {
            SetLastModificationTime(targetObject);
            SetLastModifierId(targetObject);
            if (targetObject is IModificationAuditedName userNameEntity)
            {
                userNameEntity.LastModifierUserName = GetUserName();
            }
        }

        public void SetDeletionProperties(object targetObject)
        {
            SetDeletionTime(targetObject);
            SetDeleterId(targetObject);

            if (targetObject is IDeletionAuditedName userNameEntity)
            {
                userNameEntity.DeleterUserName = GetUserName();
            }
        }

        private void SetCreationTime(object targetObject)
        {
            if (!(targetObject is IHasCreationTime objectWithCreationTime))
            {
                return;
            }

            if (objectWithCreationTime.CreationTime == default)
            {
                objectWithCreationTime.CreationTime = Clock.Now;
            }
        }
        /// <summary>
        /// 用户名
        /// </summary>
        protected virtual string GetUserName() => CurrentUser.UserName;

        /// <summary>
        /// userId
        /// </summary>
        protected virtual string GetUserId() => CurrentUser.UserId;
        private void SetCreatorId(object targetObject)
        {
            string userId = GetUserId();
            if (userId == null) return;

            switch (targetObject)
            {
                case ICreationAudited<string> strAudit:
                    if (strAudit.CreatorUserId == default)
                    {
                        strAudit.CreatorUserId = userId;
                    }
                    break;
                case ICreationAudited<int> intAudit:
                    if (intAudit.CreatorUserId == default && int.TryParse(userId, out var intId))
                    {
                        intAudit.CreatorUserId = intId;
                    }
                    break;
                case ICreationAudited<int?> nullIntAudit:
                    if (nullIntAudit.CreatorUserId == default && int.TryParse(userId, out intId))
                    {
                        nullIntAudit.CreatorUserId = intId;
                    }
                    break;
                case ICreationAudited<long> longAudit:
                    if (longAudit.CreatorUserId == default && long.TryParse(userId, out var longId))
                    {
                        longAudit.CreatorUserId = longId;
                    }
                    break;
                case ICreationAudited<long?> nullLongAudit:
                    if (nullLongAudit.CreatorUserId == default && long.TryParse(userId, out longId))
                    {
                        nullLongAudit.CreatorUserId = longId;
                    }
                    break;
                case ICreationAudited<Guid> guidAudit:
                    if (guidAudit.CreatorUserId == default && Guid.TryParse(userId, out var guidId))
                    {
                        guidAudit.CreatorUserId = guidId;
                    }
                    break;
                case ICreationAudited<Guid?> nullGuidAudit:
                    if (nullGuidAudit.CreatorUserId == default && Guid.TryParse(userId, out guidId))
                    {
                        nullGuidAudit.CreatorUserId = guidId;
                    }
                    break;
            }
        }

        private void SetLastModificationTime(object targetObject)
        {
            if (targetObject is IHasModificationTime objectWithModificationTime)
            {
                objectWithModificationTime.LastModificationTime = Clock.Now;
            }
        }

        private void SetLastModifierId(object targetObject)
        {
            string userId = GetUserId();
            if (userId == null) return;

            switch (targetObject)
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
            }
        }

        private void SetDeletionTime(object targetObject)
        {
            if (targetObject is IHasDeletionTime objectWithDeletionTime)
            {
                if (objectWithDeletionTime.DeletionTime == null)
                {
                    objectWithDeletionTime.DeletionTime = Clock.Now;
                }
            }
        }

        private void SetDeleterId(object targetObject)
        {
            string userId = GetUserId();
            if (userId == null) return;

            switch (targetObject)
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
            }
        }
    }
}

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AIoT.Core.Entities.Auditing;
using AIoT.Core.Runtime;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;

namespace AIoT.EntityFramework.EntityFrameworkCore
{
    
    public class AbpDbContext<TDbContext> : DbContext,ITransientDependency
        where TDbContext : DbContext
    {
        /// <summary>
        /// 属性注入 <see cref="ICurrentUser"/>
        /// </summary>
        public ICurrentUser CurrentUser { protected get; set; }
        /// <summary>
        /// 属性注入 <see cref=""/>
        /// </summary>
        public IDataFilter DataState { protected get; set; }
        public AbpDbContext(DbContextOptions<TDbContext> options)
            : base(options)
        {
            CurrentUser= NullCurrentUser.Instanse;
        }
        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                ConfigureGlobalMethodInfo
                    .MakeGenericMethod(entityType.ClrType)
                    .Invoke(this, new object[] { modelBuilder, modelBuilder.Entity(entityType.ClrType) });
            }
        }
        #region Audit审计

        /// <inheritdoc />
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            ApplyEntityChanges();

            var result = base.SaveChanges(acceptAllChangesOnSuccess);

            return result;
        }

        /// <inheritdoc />
        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            ApplyEntityChanges();

            var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

            return result;
        }

        /// <summary>
        /// 处理实体变更操作
        /// </summary>
        protected virtual void ApplyEntityChanges()
        {
            var entityEntries = ChangeTracker.Entries();
            foreach (var entry in entityEntries)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        ApplyForAddedEntity(entry);
                        break;
                    case EntityState.Modified:
                        ApplyForModifiedEntity(entry);
                        break;
                    case EntityState.Deleted:
                        ApplyForDeletedEntity(entry);
                        break;
                }
            }
        }

        /// <summary>
        /// 处理添加实体操作
        /// </summary>
        protected virtual void ApplyForAddedEntity(EntityEntry entry)
        {
            EntityAuditingHelper.SetCreationAuditProperties(entry.Entity, CurrentUser.UserId, CurrentUser.Name);
            EntityAuditingHelper.SetModificationAuditProperties(entry.Entity, CurrentUser.UserId, CurrentUser.Name);
        }

        /// <summary>
        /// 处理修改实体操作
        /// </summary>
        protected virtual void ApplyForModifiedEntity(EntityEntry entry)
        {
            EntityAuditingHelper.SetModificationAuditProperties(entry.Entity, CurrentUser.UserId, CurrentUser.Name);
        }

        /// <summary>
        /// 处理删除实体操作
        /// </summary>
        protected virtual void ApplyForDeletedEntity(EntityEntry entry)
        {
            if (!(entry.Entity is ISoftDelete))
                return;

            entry.Reload();
            ((ISoftDelete)entry.Entity).IsDeleted = true;
            EntityAuditingHelper.SetDeletionAuditProperties(entry.Entity, CurrentUser.UserId, CurrentUser.Name);
            EntityAuditingHelper.SetModificationAuditProperties(entry.Entity, CurrentUser.UserId, CurrentUser.Name);
        }

        #endregion


        /// <summary>
        /// 全局实体配置方法
        /// </summary>
        private static readonly MethodInfo ConfigureGlobalMethodInfo = typeof(AbpDbContext<TDbContext>)
            .GetTypeInfo().GetDeclaredMethod(nameof(CongifureGlobal));

        /// <summary>
        /// 全局实体配置方法
        /// </summary>
        public virtual void CongifureGlobal<TEntity>(ModelBuilder builder, EntityTypeBuilder entityBuilder)
            where TEntity : class
        {
            var filterExpression = CreateFilterExpression<TEntity>();
            if (filterExpression != null)
            {
                entityBuilder.HasQueryFilter(filterExpression);
            }
        }

        /// <summary>
        /// 创建实体全局过滤条件
        /// </summary>
        protected virtual Expression<Func<TEntity, bool>> CreateFilterExpression<TEntity>()
            where TEntity : class
        {
            
            Expression<Func<TEntity, bool>> expression = a => true; 
            if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
            {
                Expression<Func<TEntity, bool>> softDeleteFilter = e =>
                    !AuditDataFilterEnabled(nameof(ISoftDelete)) || ((ISoftDelete)e).IsDeleted == false;

                expression = softDeleteFilter.And(expression);
            }

           
            return expression;
        }
        /// <summary>
        /// 指定类型数据权限是否启用
        /// </summary>
        public virtual bool AuditDataFilterEnabled(string type)
        {
            return DataState.IsEnabled(type);
        }
    }


}

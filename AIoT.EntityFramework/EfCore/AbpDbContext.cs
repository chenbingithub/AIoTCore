using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AIoT.Core;
using AIoT.Core.Auditing;
using AIoT.Core.Data;
using AIoT.Core.Entities;
using AIoT.Core.EventBus.Local;
using AIoT.Core.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Volo.Abp.DependencyInjection;

namespace AIoT.EntityFramework.EfCore
{
    public abstract class AbpDbContext<TDbContext> : DbContext, IAbpEfCoreDbContext,ITransientDependency
        where TDbContext : DbContext
    {

        private const string ModelDatabaseProviderAnnotationKey = "_AIoT_DatabaseProvider";
        protected virtual bool IsSoftDeleteFilterEnabled => DataFilter?.IsEnabled<ISoftDelete>() ?? false;



        public IDataFilter DataFilter { get; set; }

        /// <summary>
        /// 实体变更事件help
        /// </summary>
        public IEntityChangeEventHelper EntityChangeEventHelper { protected get; set; }

        /// <summary>
        /// 属性注入 <see cref="AuditPropertySetter"/>
        /// </summary>
        public IAuditPropertySetter AuditPropertySetter { get; set; }


        public ILogger<AbpDbContext<TDbContext>> Logger { get; set; }

       


        protected AbpDbContext(DbContextOptions<TDbContext> options)
            : base(options)
        {
            Logger = NullLogger<AbpDbContext<TDbContext>>.Instance;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //map
            modelBuilder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);

            base.OnModelCreating(modelBuilder);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                ConfigureBaseGlobalFilters
                    .MakeGenericMethod(entityType.ClrType)
                    .Invoke(this, new object[] { modelBuilder, entityType });
            }
        }

  
        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            try
            {
                ApplyEntityChanges();
                var eventDatas = ChangeTracker.Entries().Select(p => (p.Entity, p.State, p.Metadata.ClrType)).ToList();
                var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
               
                await PublishEventsAsync(eventDatas);

                return result;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                ChangeTracker.AutoDetectChangesEnabled = true;
            }
        }

        public async  Task<int> SaveChangesOnDbContextAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
           return await this.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        public virtual void Initialize(AbpEfCoreDbContextInitializationContext initializationContext)
        {
            if (initializationContext.UnitOfWork.Options.Timeout.HasValue &&
                !Database.GetCommandTimeout().HasValue)
            {
                Database.SetCommandTimeout(initializationContext.UnitOfWork.Options.Timeout.Value);
            }

            ChangeTracker.CascadeDeleteTiming = CascadeTiming.OnSaveChanges;

            //ChangeTracker.Tracked += ChangeTracker_Tracked;
        }

        //protected virtual void ChangeTracker_Tracked(object sender, EntityTrackedEventArgs e)
        //{

        //}

        /// <summary>
        /// 发布到EnitityEvent
        /// </summary>
        private async Task PublishEventsAsync(List<(object Entity, EntityState State, Type Type)> eventDatas)
        {
            foreach (var (entity, state, type) in eventDatas)
            {
                await EntityChangeEventHelper.TriggerEntityChangedEvent(entity, state, type);
            }
        }



        /// <summary>
        /// 处理实体变更操作
        /// </summary>
        protected virtual void ApplyEntityChanges()
        {
            var entityEntries = ChangeTracker.Entries().ToList();
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
            AuditPropertySetter?.SetCreationProperties(entry.Entity);
            AuditPropertySetter?.SetModificationProperties(entry.Entity);
        }

        /// <summary>
        /// 处理修改实体操作
        /// </summary>
        protected virtual void ApplyForModifiedEntity(EntityEntry entry)
        {
            AuditPropertySetter?.SetModificationProperties(entry.Entity);
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
            AuditPropertySetter?.SetDeletionProperties(entry.Entity);
            AuditPropertySetter?.SetModificationProperties(entry.Entity);
        }

        private static readonly MethodInfo ConfigureBaseGlobalFilters
            = typeof(AbpDbContext<TDbContext>)
                .GetMethod(
                    nameof(ConfigureGlobalFilters),
                    BindingFlags.Instance | BindingFlags.NonPublic
                );


        protected virtual void ConfigureGlobalFilters<TEntity>(ModelBuilder modelBuilder ,IMutableEntityType mutableEntityType)
            where TEntity : class
        {
            var filterExpression = CreateFilterExpression<TEntity>();
            if (filterExpression != null)
            {
                modelBuilder.Entity<TEntity>().HasQueryFilter(filterExpression);
            }
        }

        protected virtual Expression<Func<TEntity, bool>> CreateFilterExpression<TEntity>()
            where TEntity : class
        {
            Expression<Func<TEntity, bool>> expression = null;

            if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
            {
                expression = e => !IsSoftDeleteFilterEnabled || !EF.Property<bool>(e, "IsDeleted");
            }
            return expression;
        }

        

        
    }
}

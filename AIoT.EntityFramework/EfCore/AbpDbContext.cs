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
        /// 本地事件总线
        /// </summary>
        public ILocalEventBus LocalEventBus { protected get; set; } = NullLocalEventBus.Instanse;

        /// <summary>
        /// 属性注入 <see cref="AuditPropertySetter"/>
        /// </summary>
        public IAuditPropertySetter AuditPropertySetter { get; set; }


        public ILogger<AbpDbContext<TDbContext>> Logger { get; set; }

        private static readonly MethodInfo ConfigureBasePropertiesMethodInfo
            = typeof(AbpDbContext<TDbContext>)
                .GetMethod(
                    nameof(ConfigureBaseProperties),
                    BindingFlags.Instance | BindingFlags.NonPublic
                );


        protected AbpDbContext(DbContextOptions<TDbContext> options)
            : base(options)
        {
            Logger = NullLogger<AbpDbContext<TDbContext>>.Instance;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

           // TrySetDatabaseProvider(modelBuilder);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                ConfigureBasePropertiesMethodInfo
                    .MakeGenericMethod(entityType.ClrType)
                    .Invoke(this, new object[] { modelBuilder, entityType });



            }
        }

        protected virtual void TrySetDatabaseProvider(ModelBuilder modelBuilder)
        {
            var provider = GetDatabaseProviderOrNull(modelBuilder);
            if (provider != null)
            {
                modelBuilder.Model.SetAnnotation(ModelDatabaseProviderAnnotationKey, provider.Value);
            }
        }

        protected virtual EfCoreDatabaseProvider? GetDatabaseProviderOrNull(ModelBuilder modelBuilder)
        {
            switch (Database.ProviderName)
            {
                case "Microsoft.EntityFrameworkCore.SqlServer":
                    return EfCoreDatabaseProvider.SqlServer;
                case "Npgsql.EntityFrameworkCore.PostgreSQL":
                    return EfCoreDatabaseProvider.PostgreSql;
                case "Pomelo.EntityFrameworkCore.MySql":
                    return EfCoreDatabaseProvider.MySql;
                case "Oracle.EntityFrameworkCore":
                case "Devart.Data.Oracle.Entity.EFCore":
                    return EfCoreDatabaseProvider.Oracle;
                case "Microsoft.EntityFrameworkCore.Sqlite":
                    return EfCoreDatabaseProvider.Sqlite;
                case "Microsoft.EntityFrameworkCore.InMemory":
                    return EfCoreDatabaseProvider.InMemory;
                case "FirebirdSql.EntityFrameworkCore.Firebird":
                    return EfCoreDatabaseProvider.Firebird;
                case "Microsoft.EntityFrameworkCore.Cosmos":
                    return EfCoreDatabaseProvider.Cosmos;
                default:
                    return null;
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

            ChangeTracker.Tracked += ChangeTracker_Tracked;
        }

        protected virtual void ChangeTracker_Tracked(object sender, EntityTrackedEventArgs e)
        {
            var entityType = e.Entry.Metadata.ClrType;
            if (entityType == null)
            {
                return;
            }


            if (!e.FromQuery)
            {
                return;
            }
        }

        

        /// <summary>
        /// 发布到EnitityEvent
        /// </summary>
        private async Task PublishEventsAsync(List<(object Entity, EntityState State, Type Type)> eventDatas)
        {
            foreach (var (entity, state, type) in eventDatas)
            {
                //var method = _publishEventAsync.MakeGenericMethod(type);
                //await (Task)method.Invoke(this, new[] { entity, state });
               await  PublishEventAsync(entity, state);
            }
        }

        private static readonly MethodInfo _publishEventAsync = typeof(AbpDbContext<>)
            .GetMethod(nameof(PublishEventAsync), BindingFlags.Instance | BindingFlags.NonPublic);
        private async Task PublishEventAsync<TEntity>(TEntity entity, EntityState state)
        {
            switch (state)
            {
                case EntityState.Added:
                    await LocalEventBus.PublishAsync(new EntityCreatedEventData<TEntity>(entity));
                    break;
                case EntityState.Modified:
                    await LocalEventBus.PublishAsync(new EntityUpdatedEventData<TEntity>(entity));
                    break;
                case EntityState.Deleted:
                    await LocalEventBus.PublishAsync(new EntityDeletedEventData<TEntity>(entity));
                    break;
                default:break;
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

       
       
        protected virtual void ConfigureBaseProperties<TEntity>(ModelBuilder modelBuilder, IMutableEntityType mutableEntityType)
            where TEntity : class
        {
            
            if (!typeof(IEntity).IsAssignableFrom(typeof(TEntity)))
            {
                return;
            }

          //  modelBuilder.Entity<TEntity>().ConfigureByConvention();

            ConfigureGlobalFilters<TEntity>(modelBuilder, mutableEntityType);
        }

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

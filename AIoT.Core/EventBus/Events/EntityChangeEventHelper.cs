using System;
using System.Reflection;
using System.Threading.Tasks;
using AIoT.Core.EventBus.Local;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.DependencyInjection;

namespace AIoT.Core.EventBus.Events
{
    /// <inheritdoc cref="IEntityChangeEventHelper" />
    public class EntityChangeEventHelper : IEntityChangeEventHelper, ITransientDependency
    {
        private readonly ILocalEventBus _localEventBus;

        /// <inheritdoc cref="EntityChangeEventHelper"/>
        public EntityChangeEventHelper(ILocalEventBus localEventBus)
        {
            _localEventBus = localEventBus;
        }

        /// <inheritdoc  />
        public virtual async Task TriggerEntityChangedEvent(object entity, EntityState state, Type entityType)
        {
            await TriggerEventWithEntity(entity, state, entityType);
        }

        /// <summary>
        /// 触发实体变更事件
        /// </summary>
        protected virtual async Task TriggerEventWithEntity(object entity, EntityState state, Type entityType)
        {
            var method = _publishEventAsync.MakeGenericMethod(entityType);
            // ReSharper disable once PossibleNullReferenceException
            await ((Task)method.Invoke(this, new[] { entity, state }));
        }
        private static readonly MethodInfo _publishEventAsync = typeof(EntityChangeEventHelper)
            .GetMethod(nameof(PublishEventAsync), BindingFlags.Instance | BindingFlags.NonPublic);

        protected async Task PublishEventAsync<TEntity>(TEntity entity, EntityState state)
        {
            switch (state)
            {
                case EntityState.Added:
                    await _localEventBus.PublishAsync(new EntityCreatedEventData<TEntity>(entity));
                    break;
                case EntityState.Modified:
                    await _localEventBus.PublishAsync(new EntityUpdatedEventData<TEntity>(entity));
                    break;
                case EntityState.Deleted:
                    await _localEventBus.PublishAsync(new EntityDeletedEventData<TEntity>(entity));
                    break;
            }
        }
    }
}

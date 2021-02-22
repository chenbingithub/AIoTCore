using System;

namespace AIoT.Core.EventBus.Events
{

    /// <summary>
    /// 更新实体事件数据
    /// </summary>
    [Serializable]
    public class EntityUpdatedEventData<TEntity> : EntityChangedEventData<TEntity>
    {
        /// <inheritdoc />
        public EntityUpdatedEventData(TEntity entity) : base(entity)
        {
        }
    }
}

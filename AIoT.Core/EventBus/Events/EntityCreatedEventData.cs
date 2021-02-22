using System;

namespace AIoT.Core.EventBus.Events
{


    /// <summary>
    /// 新增实体事件数据
    /// </summary>
    [Serializable]
    public class EntityCreatedEventData<TEntity> : EntityChangedEventData<TEntity>
    {
        /// <inheritdoc />
        public EntityCreatedEventData(TEntity entity) : base(entity)
        {
        }
    }
}

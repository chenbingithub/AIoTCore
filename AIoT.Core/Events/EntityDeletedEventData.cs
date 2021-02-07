﻿using System;

 namespace AIoT.Core.Events
{

    /// <summary>
    /// 删除实体事件数据
    /// </summary>
    [Serializable]
    public class EntityDeletedEventData<TEntity> : EntityChangedEventData<TEntity>
    {
        /// <inheritdoc />
        public EntityDeletedEventData(TEntity entity) : base(entity)
        {
        }
    }
}

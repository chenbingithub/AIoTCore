﻿using System;

 namespace AIoT.Core.Events
{
   
    /// <summary>
    /// 实体变更事件参数基类
    /// </summary>
    [Serializable]
    public class EntityChangedEventData<TEntity> : EntityEventData<TEntity>{
        /// <inheritdoc />
        public EntityChangedEventData(TEntity entity) : base(entity)
        {
        }
    }
}

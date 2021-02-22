﻿using System;

 namespace AIoT.Core.EventBus.Events
{
    /// <summary>
    /// 实体事件参数基类
    /// </summary>
    public interface IEntityEventData<out TEntity>
    {
        /// <summary>
        /// 实体
        /// </summary>
        TEntity Entity { get; }
    }

    /// <summary>
    /// 实体事件参数基类
    /// </summary>
    [Serializable]
    public class EntityEventData< TEntity> : IEntityEventData<TEntity>
    {
        public EntityEventData(TEntity entity)
        {
            Entity = entity;
        }

        /// <summary>
        /// 实体
        /// </summary>
        public TEntity Entity { get; }
    }
}

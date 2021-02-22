using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace AIoT.Core.EventBus.Events
{
    /// <summary>
    /// 实体变更事件
    /// </summary>
    public interface IEntityChangeEventHelper
    {
        /// <summary>
        /// 触发实体变更事件
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="state"></param>
        /// <param name="entityType"></param>
        /// <returns></returns>
        Task TriggerEntityChangedEvent(object entity, EntityState state, Type entityType);
    }
}

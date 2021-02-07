using System;
using System.Collections.Generic;
using System.Text;

namespace AIoT.Core.EventBus.Local
{
    /// <summary>
    /// 本地事件总线配置
    /// </summary>
    public class LocalEventBusOptions
    {
        /// <summary>
        /// 事件订阅处理者
        /// </summary>
        public IDictionary<Type, IEnumerable<Type>> Handlers { get; } = new Dictionary<Type, IEnumerable<Type>>();
    }
}

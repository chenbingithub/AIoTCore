using System;

namespace AIoT.Core.Cache.Internal
{
    /// <summary>
    /// 元数据
    /// </summary>
    public class CacheMeta
    {
        /// <summary>
        /// 绝对过期时间
        /// </summary>
        public DateTimeOffset? AbsoluteExpire { get; set; }

        /// <summary>
        /// 滑动过期时间
        /// </summary>
        public TimeSpan? SlidingExpire { get; set; }

        /// <summary>
        /// 剩余存活时间
        /// </summary>
        public DateTimeOffset? TimeToLive { get; set; }
    }
}
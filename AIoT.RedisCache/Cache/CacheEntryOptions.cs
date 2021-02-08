﻿using System;

namespace AIoT.RedisCache.Cache
{
    /// <summary>
    /// 缓存配置
    /// </summary>
    public class CacheEntryOptions
    {
        private TimeSpan? _absoluteExpire;
        private TimeSpan? _slidingExpire;

        /// <summary>
        /// 决对过期时间
        /// </summary>
        public TimeSpan? AbsoluteExpire
        {
            get => _absoluteExpire;
            set
            {
                if (value.HasValue && value.Value <= TimeSpan.Zero)
                    throw new ArgumentOutOfRangeException(nameof(AbsoluteExpire), value, "过期时间无效");
                _absoluteExpire = value;
            }
        }

        /// <summary>
        /// 滑动过期时间
        /// </summary>
        public TimeSpan? SlidingExpire
        {
            get => _slidingExpire;
            set
            {
                if (value.HasValue && value.Value <= TimeSpan.Zero)
                    throw new ArgumentOutOfRangeException(nameof(SlidingExpire), value, "过期时间无效");
                _slidingExpire = value;
            }
        }
    }

    /// <summary>
    /// 缓存配置
    /// </summary>
    public class CacheEntryConfigOptions : CacheEntryOptions
    {
        /// <summary>
        /// 缓存存储策略
        /// </summary>
        public CacheStoragePolicy StoragePolicy { get; set; } = CacheStoragePolicy.MemoryAndRedis;
    }
    /// <summary>
    /// 缓存存储策略
    /// </summary>
    [Flags]
    public enum CacheStoragePolicy
    {
        /// <summary>
        /// 内存
        /// </summary>
        Memory = 1,

        /// <summary>
        /// Redis
        /// </summary>
        Redis = 2,

        /// <summary>
        /// 内存加Redis
        /// </summary>
        MemoryAndRedis = Memory | Redis,
    }
}

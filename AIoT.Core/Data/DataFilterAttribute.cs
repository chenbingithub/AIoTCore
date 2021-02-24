﻿using System;

 namespace AIoT.Core.Data
{
    /// <summary>
    /// 状态开关
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class DataFilterAttribute : Attribute
    {
        /// <summary>
        /// 名称
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enable { get; set; }
        
        /// <summary>
        /// 状态开关
        /// </summary>
        protected DataFilterAttribute() { }

        /// <summary>
        /// 状态开关
        /// </summary>
        public DataFilterAttribute(Type type, bool enable)
        {
            Type = type;
            Enable = enable;
        }
    }
}

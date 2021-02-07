using System;
using System.Collections.Generic;

namespace AIoT.Core.Data
{
   
    /// <summary>
    /// 连接字符串配置
    /// </summary>
    [Serializable]
    public class ConnectionStrings : Dictionary<string, string>
    {
        /// <summary>
        /// 默认连接字符串名称
        /// </summary>
        public const string DefaultConnectionStringName = "Default";

        /// <summary>
        /// 默认只读连接字符串名称
        /// </summary>
        public const string DefaultReadonlyConnectionStringName = "Readonly";

        /// <summary>
        /// 获取默认连接字符串
        /// </summary>
        public string Default
        {
            get => this.GetValueOrDefault(DefaultConnectionStringName);
            set => this[DefaultConnectionStringName] = value;
        }

        /// <summary>
        /// 获取默认只读连接字符串
        /// </summary>
        public string Readonly
        {
            get => this.GetValueOrDefault(DefaultReadonlyConnectionStringName);
            set => this[DefaultReadonlyConnectionStringName] = value;
        }

 
    }
}
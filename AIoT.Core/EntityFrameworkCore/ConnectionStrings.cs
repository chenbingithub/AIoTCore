using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AIoT.Core.EntityFrameworkCore
{
    /// <summary>
    /// 连接字符串配置
    /// </summary>
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
        [JsonIgnore]
        public string Default
        {
            get => this.GetValueOrDefault(DefaultConnectionStringName);
            set => this[DefaultConnectionStringName] = value;
        }

        /// <summary>
        /// 获取默认只读连接字符串
        /// </summary>
        [JsonIgnore]
        public string Readonly
        {
            get => this.GetValueOrDefault(DefaultReadonlyConnectionStringName);
            set => this[DefaultReadonlyConnectionStringName] = value;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AIoT.Core.AutoMap
{
    /// <summary>
    /// Auto Map 属性映射
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class MapFromAttribute : Attribute
    {
        /// <summary>
        /// 对应属性路径
        /// </summary>
        public string[] PropertyPath { get; set; }

        /// <summary>
        /// 属性映射
        /// </summary>
        public MapFromAttribute(params string[] propertyPath)
        {
            propertyPath = propertyPath?.SelectMany(p => p.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries)).ToArray();
            if (propertyPath?.Length > 0)
                PropertyPath = propertyPath;
        }
    }
}

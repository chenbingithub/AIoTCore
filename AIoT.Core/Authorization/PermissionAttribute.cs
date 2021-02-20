using System;
using System.Collections.Generic;

namespace AIoT.Core.Authorization
{
    /// <summary>
    /// 功能权限验证
    /// </summary>
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public  class PermissionAttribute : Attribute
    {
        /// <summary>
        /// 权限编码：模块名称_业务名称_业务功能
        /// </summary>
        public IList<string> Permissions { get; }

        /// <summary>
        /// 功能权限验证
        /// </summary>
        /// <param name="permissions">权限编码：模块名称_业务名称_业务功能</param>
        public PermissionAttribute(params string[] permissions)
        {
            Permissions = permissions ?? new string[0];
        }
    }
}

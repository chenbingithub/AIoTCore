using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace AIoT.Core.Web
{
    /// <summary></summary>
    public static class ActionDescriptorExtensions
    {
        /// <summary>
        /// 转换为 <see cref="ControllerActionDescriptor"/>
        /// </summary>
        public static ControllerActionDescriptor AsControllerActionDescriptor(this ActionDescriptor actionDescriptor)
        {
            var controllerActionDescriptor = actionDescriptor as ControllerActionDescriptor;
            if (controllerActionDescriptor == null)
            {
                throw new InvalidOperationException($"{nameof(actionDescriptor)}:{actionDescriptor.GetType().FullName} should be type of {typeof(ControllerActionDescriptor).AssemblyQualifiedName}");
            }

            return controllerActionDescriptor;
        }

        /// <summary>
        /// 获取 Action 的 MethodInfo
        /// </summary>
        public static MethodInfo GetMethodInfo(this ActionDescriptor actionDescriptor)
        {
            return actionDescriptor.AsControllerActionDescriptor().MethodInfo;
        }

        /// <summary>
        /// 获取 Controller 和 Action 的 Attribute
        /// </summary>
        public static IEnumerable<object> GetControllerAndActionAttributes(this ControllerActionDescriptor descriptor, bool inherit)
        {
            var customAttributes = descriptor.ControllerTypeInfo.GetCustomAttributes(inherit);
            var actionAttributes = descriptor.MethodInfo.GetCustomAttributes(inherit);

            return customAttributes.Union(actionAttributes);
        }

        /// <summary>
        /// 获取 Controller 和 Action 的 Attribute
        /// </summary>
        public static IEnumerable<T> GetControllerAndActionAttributes<T>(this ControllerActionDescriptor descriptor, bool inherit)
            where T : Attribute
        {
            var customAttributes = descriptor.ControllerTypeInfo.GetCustomAttributes<T>(inherit);
            var actionAttributes = descriptor.MethodInfo.GetCustomAttributes<T>(inherit);

            return customAttributes.Union(actionAttributes);
        }
    }
}

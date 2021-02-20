using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AIoT.Core.Dto;
using Volo.Abp.DependencyInjection;
using Volo.Abp.DynamicProxy;

namespace AIoT.Core.Authorization
{
    /// <summary>
    /// 功能权限拦截器
    /// </summary>
    public class PermissionInterceptor : AbpInterceptor, ITransientDependency
    {
        private readonly IPermissionChecker _permissionChecker;

        public PermissionInterceptor(IPermissionChecker permissionChecker)
        {
            _permissionChecker = permissionChecker;
        }


        public override async Task InterceptAsync(IAbpMethodInvocation invocation)
        {
            foreach (var attribute in GetPermissionAttribute(invocation.Method))
            {
                if (!attribute.Permissions.Any()) continue;

                foreach (var item in attribute.Permissions)
                {
                    if (item != null && await _permissionChecker.IsGrantedAsync(item))
                    {
                        goto Lbl_Process;
                    }
                }

                // 权限不足
                var permissions = string.Join(",", attribute.Permissions);
                throw new ResultException(ResultCode.Forbidden, $"用户权限不足：{permissions}");
            }

            Lbl_Process:
            await invocation.ProceedAsync();
        }

        internal static IEnumerable<PermissionAttribute> GetPermissionAttribute(MethodInfo method)
        {
            // 方法
            var attrs = method.GetCustomAttributes<PermissionAttribute>(true);
            if (method.ReflectedType != null)
            {
                attrs = method.ReflectedType.GetCustomAttributes<PermissionAttribute>(true).Concat(attrs);
            }
            
            return attrs;
        }

        internal static bool PermissionDefined(TypeInfo type)
        {
            // 是否定义了 PermissionAttribute
            if (HasPermissionAttribute(type) || AnyMethodHasPermissionAttribute(type))
            {
                return true;
            }

            return false;
        }

        private static bool AnyMethodHasPermissionAttribute(TypeInfo type)
        {
            return type
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Any(HasPermissionAttribute);
        }

        private static bool HasPermissionAttribute(MemberInfo memberInfo)
        {
            return memberInfo.IsDefined(typeof(PermissionAttribute), true);
        }
    }
}

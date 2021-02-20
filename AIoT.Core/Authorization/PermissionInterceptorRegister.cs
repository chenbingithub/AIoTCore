using System.Reflection;
using Volo.Abp.DependencyInjection;

namespace AIoT.Core.Authorization
{
    /// <summary>
    /// 功能权限拦截器
    /// </summary>
    public static class PermissionInterceptorRegister
    {
        /// <summary>
        /// 功能权限拦截器
        /// </summary>
        public static void RegisterIfNeeded(IOnServiceRegistredContext context)
        {
            if (PermissionInterceptor.PermissionDefined(context.ImplementationType.GetTypeInfo()))
            {
                context.Interceptors.TryAdd<PermissionInterceptor>();
            }
        }
    }
}

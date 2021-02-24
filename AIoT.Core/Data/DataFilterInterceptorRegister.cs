using System.Reflection;
using Volo.Abp.DependencyInjection;

namespace AIoT.Core.Data
{
    /// <summary>
    /// 状态开关拦截器
    /// </summary>
    public static class DataFilterInterceptorRegister
    {
        /// <summary>
        /// 状态开关拦截器
        /// </summary>
        public static void RegisterIfNeeded(IOnServiceRegistredContext context)
        {
            if (DataFilterInterceptor.DataStateDefined(context.ImplementationType.GetTypeInfo()))
            {
                context.Interceptors.TryAdd<DataFilterInterceptor>();
            }
        }
    }
}

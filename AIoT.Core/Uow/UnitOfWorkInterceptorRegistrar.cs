using System.Reflection;
using Volo.Abp.DependencyInjection;

namespace AIoT.Core.Uow
{
    public static class UnitOfWorkInterceptorRegistrar
    {
        public static void RegisterIfNeeded(IOnServiceRegistredContext context)
        {
            if (UnitOfWorkHelper.IsUnitOfWorkType(context.ImplementationType.GetTypeInfo()))
            {
                context.Interceptors.TryAdd<UnitOfWorkInterceptor>();
            }
        }
    }
}
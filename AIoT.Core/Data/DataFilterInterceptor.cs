using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.DynamicProxy;

namespace AIoT.Core.Data
{
    /// <summary>
    /// 开关状态拦截器
    /// </summary>
    public class DataFilterInterceptor : AbpInterceptor, ITransientDependency
    {
        private readonly IDataFilter _dataFilter;

        public DataFilterInterceptor( IDataFilter dataFilter)
        {
            _dataFilter = dataFilter;
        }


        public override async Task InterceptAsync(IAbpMethodInvocation invocation)
        {
            var disposList = GetDataStateAttribute(invocation.Method)
                .Select(p => _dataFilter.UseData(p.Type,p.Enable)).ToArray();
            try
            {
                await invocation.ProceedAsync();
            }
            finally
            {
                foreach (var disposable in disposList)
                {
                    disposable.Dispose();
                }
            }
        }

        internal static IEnumerable<DataFilterAttribute> GetDataStateAttribute(MethodInfo method)
        {
            // 方法
            var attrs = method.GetCustomAttributes<DataFilterAttribute>(true);
            if (method.ReflectedType != null)
            {
                attrs = method.ReflectedType.GetCustomAttributes<DataFilterAttribute>(true).Concat(attrs);
            }
            
            return attrs;
        }

        internal static bool DataStateDefined(TypeInfo type)
        {
            // 是否定义了 DataStateAttribute
            if (HasDataStateAttribute(type) || AnyMethodHasDataStateAttribute(type))
            {
                return true;
            }

            return false;
        }

        private static bool AnyMethodHasDataStateAttribute(TypeInfo type)
        {
            return type
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Any(HasDataStateAttribute);
        }

        private static bool HasDataStateAttribute(MemberInfo memberInfo)
        {
            return memberInfo.IsDefined(typeof(DataFilterAttribute), true);
        }
    }
}

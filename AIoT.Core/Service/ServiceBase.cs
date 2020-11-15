using AIoT.Core.Uow;
using Volo.Abp.DependencyInjection;

namespace AIoT.Core.Service
{
    /// <summary>
    /// 应用服务基类
    /// </summary>
    public class ServiceBase : IServiceBase, ITransientDependency, IUnitOfWorkEnabled
    {

    }
}
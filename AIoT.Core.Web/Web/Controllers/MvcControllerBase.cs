using Microsoft.AspNetCore.Mvc;
using Volo.Abp.DependencyInjection;

namespace AIoT.Core.Web
{
    /// <summary>
    /// Mvc控制器基类
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
    public abstract class MvcControllerBase : Controller, ITransientDependency
    {
    }
}

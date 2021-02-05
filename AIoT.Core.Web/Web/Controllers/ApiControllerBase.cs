using Microsoft.AspNetCore.Mvc;
using Volo.Abp.DependencyInjection;

namespace AIoT.Core.Web
{
    /// <summary>
    /// Api控制器基类
    /// </summary>
    [ApiController]
    //[Consumes("application/json", "text/json")]
    [TypeFilter(typeof(HttpLogMiddleware.HttpLogFilter))]
    [TypeFilter(typeof(ExceptionResultFilter))]
    public abstract class ApiControllerBase : ControllerBase, ITransientDependency
    {
    }
}

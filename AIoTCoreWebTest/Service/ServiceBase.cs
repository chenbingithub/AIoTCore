using AIoT.Core.EntityFrameworkCore;
using AIoT.Core.Uow;
using Volo.Abp.DependencyInjection;

namespace AIoT.Core.Service
{
    /// <summary>
    /// 应用服务基类
    /// </summary>
    public class MyService:ServiceBase,IMyService
    {
        private readonly IDbContextProvider _contextProvider;

        public MyService(IDbContextProvider contextProvider)
        {
            _contextProvider = contextProvider;
        }

        public string Test()
        {
           var db= _contextProvider.GetDbContext<AbpDbContext>();
           return "ok";
        }
    }
}
using System;
using AIoT.Core.Runtime;
using AIoT.Core.Web.Runtime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Volo.Abp.Modularity;


namespace AIoT.Core.Web
{
    [DependsOn(typeof(AIoTCoreModule))]
    public class AIoTCoreWebModule: AbpModule
    {
        /// <inheritdoc />
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddTransient<ExceptionResultFilter>();
            context.Services.AddTransient<ValidateResultFilter>();
   
            context.Services.AddHttpContextAccessor();
            context.Services.AddObjectAccessor<IApplicationBuilder>();
            context.Services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);
            // Configure MVC
            Configure<MvcOptions>(p =>
            {
                p.Filters.Add<ValidateResultFilter>();
                p.InputFormatters.Add(new FormDataInputFormatter());
            });
            context.Services.AddControllers().AddNewtonsoftJson((options) =>
            {
                //options.SerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
                ////设置取消循环引用
                //options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                //设置日期的格式为：yyyy-MM-dd
                options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                //设置首字母小写
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            }); ;
        }

       
    }
}

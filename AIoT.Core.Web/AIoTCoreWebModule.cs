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
            //context.Services.AddTransient<ExceptionResultFilter>();
            //context.Services.AddTransient<ValidateResultFilter>();
            context.Services.Replace(ServiceDescriptor.Transient<IPrincipalAccessor, AspNetCorePrincipalAccessor>());
            AddAspNetServices(context.Services);

            
        }

        /// <inheritdoc />
        public override void PostConfigureServices(ServiceConfigurationContext context)
        {
            //var jobServerOptions = context.Services.GetSingletonInstanceOrNull<BackgroundJobServerOptions>();
            //if (jobServerOptions != null)
            //{
            //    jobServerOptions.Queues = jobServerOptions.Queues.Distinct().ToArray();
            //}
        }

        private static void AddAspNetServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddObjectAccessor<IApplicationBuilder>();

            services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);

            //services.AddSingleton<IContentTypeProvider, FileExtensionContentTypeProvider>();
        }
    }
}

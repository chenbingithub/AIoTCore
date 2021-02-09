using System;
using AIoT.Core;
using AIoT.Core.Extensions;
using AIoT.Core.Web;
using AIoT.EntityFramework;
using AIoT.EntityFramework.EntityFrameworkCore.DependencyInjection;
using AIoTCoreWebTest.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Volo.Abp;
using Volo.Abp.Modularity;

namespace AIoTCoreWebTest
{
    [DependsOn(typeof(AIoTCoreWebModule),typeof(AbpEntityFrameworkCoreModule))]
    public class AIoTCoreWebTestModule:AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var services = context.Services;
            var config = context.Services.GetConfiguration();
            services.AddHealthChecks();
            // Routing
            services.AddRouting();
            services.AddControllers();
            services.AddAbpDbContext<MyDbContext>();
        }

        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {
            var app = AbpApplicationBuilderExtensions.GetApplicationBuilder(context);
            var env = AbpApplicationBuilderExtensions.GetEnvironment(context);
           
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Forward
            app.UseForwardedHeaders();

            // Routing
            app.UseRouting();

        


            // Execute the matched endpoint.
            app.UseEndpoints(endpoints =>
            {
                // ½¡¿µ¼ì²é
                endpoints.MapHealthChecks("/health");

               
                // MVC
                endpoints.MapDefaultControllerRoute();
            });

        }
    }
}

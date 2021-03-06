using System;
using AIoT.Core;
using AIoT.Core.EntityFrameworkCore;
using AIoT.Core.Extensions;
using AIoT.Core.Web;
using AIoTCoreWebTest.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Volo.Abp;
using Volo.Abp.Modularity;

namespace AIoTCoreWebTest
{
    [DependsOn(typeof(AIoTCoreWebModule))]
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
            services.AddAbpDbContext<EfDbContext>();
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
                // �������
                endpoints.MapHealthChecks("/health");

               
                // MVC
                endpoints.MapDefaultControllerRoute();
            });

        }
    }
}

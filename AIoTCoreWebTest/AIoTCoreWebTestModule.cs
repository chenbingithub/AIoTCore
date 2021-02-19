using System;
using AIoT.Core;
using AIoT.Core.AutoMap;
using AIoT.Core.Web;
using AIoT.EntityFramework;
using AIoT.EntityFramework.DependencyInjection;
using AIoT.RedisCache;
using AIoTCoreWebTest.Service;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Volo.Abp;
using Volo.Abp.Modularity;

namespace AIoTCoreWebTest
{
    [DependsOn(typeof(AIoTCoreWebModule),typeof(AbpEntityFrameworkCoreModule),typeof(AIoTRedisCacheModule))]
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
            context.Services.ConfigMapper(CreateDtoMappings);
        }

        private void CreateDtoMappings(IMapperConfigurationExpression s)
        {
            s.CreateMap<Role, RoleDto>();
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

using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace AIoT.Core.Extensions
{
    public static class AbpApplicationBuilderExtensions
    {
        private const string ExceptionHandlingMiddlewareMarker = "_AbpExceptionHandlingMiddleware_Added";

        public static void InitializeApplication([NotNull] this IApplicationBuilder app)
        {
            Check.NotNull(app, nameof(app));

            app.ApplicationServices.GetRequiredService<ObjectAccessor<IApplicationBuilder>>().Value = app;
            var application = app.ApplicationServices.GetRequiredService<IAbpApplicationWithExternalServiceProvider>();
            var applicationLifetime = app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>();

            applicationLifetime.ApplicationStopping.Register(() =>
            {
                application.Shutdown();
            });

            applicationLifetime.ApplicationStopped.Register(() =>
            {
                application.Dispose();
            });

            application.Initialize(app.ApplicationServices);
        }
        public static IApplicationBuilder GetApplicationBuilder(this ApplicationInitializationContext context)
        {
            return context.ServiceProvider.GetRequiredService<IObjectAccessor<IApplicationBuilder>>().Value;
        }

        public static IHostEnvironment GetEnvironment(this ApplicationInitializationContext context)
        {
            return context.ServiceProvider.GetRequiredService<IHostEnvironment>();
        }

        public static IConfiguration GetConfiguration(this ApplicationInitializationContext context)
        {
            return context.ServiceProvider.GetRequiredService<IConfiguration>();
        }

        public static ILoggerFactory GetLoggerFactory(this ApplicationInitializationContext context)
        {
            return context.ServiceProvider.GetRequiredService<ILoggerFactory>();
        }

    }
}

using System;
using AIoT.Core;
using AIoT.EntityFramework.EntityFrameworkCore;
using AIoT.EntityFramework.Uow.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Volo.Abp.Modularity;

namespace AIoT.EntityFramework
{
    [DependsOn(typeof(AIoTCoreModule))]
    public class AbpEntityFrameworkCoreModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<AbpDbContextOptions>(options =>
            {
                options.PreConfigure(opt =>
                {
                    opt.DbContextOptions
                        .ConfigureWarnings(warnings =>
                        {
                            warnings.Ignore(CoreEventId.LazyLoadOnDisposedContextWarning);
                        });
                    if (opt.ExistingConnection != null)
                        opt.DbContextOptions.UseMySQL(opt.ExistingConnection);
                    else
                        opt.DbContextOptions.UseMySQL(opt.ConnectionString);
                });
            });

            context.Services.TryAddTransient(typeof(IDbContextProvider<>), typeof(UnitOfWorkDbContextProvider<>));
        }
    }
}

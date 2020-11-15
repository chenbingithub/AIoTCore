using System;
using AIoT.Core.DataFilter;
using AIoT.Core.Entities.Auditing;
using AIoT.Core.EntityFrameworkCore;
using AIoT.Core.EntityFrameworkCore.Uow.EntityFrameworkCore;
using AIoT.Core.Enums;
using AIoT.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;
using AIoT.Core.Uow;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.EntityFrameworkCore;

namespace AIoT.Core
{
    public class AIoTCoreModule : AbpModule
    {
        public override void PreConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.OnRegistred(UnitOfWorkInterceptorRegistrar.RegisterIfNeeded);
            
           
        }
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var services = context.Services;
            var config = services.GetConfiguration();
            Configure<DbConnectionOptions>(config);
            Configure<AbpDbContextOptions>(options =>
            {
                
                options.PreConfigure(p =>
                {
                    if (p.DatabaseProvider.ToLower() == EfCoreDatabaseProvider.MySql.ToString().ToLower())
                    {
                        if (p.ExistingConnection != null)
                            p.DbContextOptions.UseMySQL(p.ExistingConnection);
                        else
                            p.DbContextOptions.UseMySQL(p.ConnectionString);
                    }else if (p.DatabaseProvider.ToLower() == EfCoreDatabaseProvider.SqlServer.ToString().ToLower())
                    {
                        if(p.ExistingConnection != null)
                            p.DbContextOptions.UseSqlServer(p.ExistingConnection);
                        else
                            p.DbContextOptions.UseSqlServer(p.ConnectionString);
                    }
                    else if(p.DatabaseProvider.ToLower() == EfCoreDatabaseProvider.Oracle.ToString().ToLower())
                    {
                        if (p.ExistingConnection != null)
                            p.DbContextOptions.UseOracle(p.ExistingConnection);
                        else
                            p.DbContextOptions.UseOracle(p.ConnectionString);
                    }
                    else
                    {
                        throw  new Exception($"数据库类型配置不正确{p.DatabaseProvider.ToLower()}");
                    }


                });
            });

            // 
            services.AddAbpDbContext<AbpDbContext>();
            
            Configure<DataStateOptions>(options =>
            {
                options.DefaultStates[nameof(ISoftDelete)] = true;
            });


            context.Services.TryAddTransient(typeof(IDbContextProvider), typeof(UnitOfWorkDbContextProvider));
        }
    }
   
}

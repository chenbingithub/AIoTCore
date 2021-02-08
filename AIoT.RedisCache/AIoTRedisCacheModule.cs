using System;
using AIoT.Core;
using Volo.Abp.Modularity;

namespace AIoT.RedisCache
{
    [DependsOn(typeof(AIoTCoreModule))]
    public class AIoTRedisCacheModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            base.ConfigureServices(context);
        }

    }
}

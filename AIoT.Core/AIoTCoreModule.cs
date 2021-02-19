using System.Data;
using AIoT.Core.Data;
using AIoT.Core.Threading;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;
using AIoT.Core.Uow;
using Volo.Abp.Autofac;
using Volo.Abp.Data;
using AutoMapper;
using AutoMapper.Configuration;
using System.Reflection;
using AIoT.Core.Dto;
using System.Linq.Expressions;
using AIoT.Core.AutoMap;
using System.Collections.Generic;
using Volo.Abp.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using IdentityModel.Client;
using System;
using AIoT.Core.Enums;

namespace AIoT.Core
{
    [DependsOn(typeof(AbpAutofacModule))]
    public class AIoTCoreModule : AbpModule
    {
        public override void PreConfigureServices(ServiceConfigurationContext context)
        {
            var services = context.Services;

            // Uow 拦截器
            services.OnRegistred(UnitOfWorkInterceptorRegistrar.RegisterIfNeeded);

            
        }
        private static bool _staticMapperInitialized;
        private static readonly object _autoMapperSyncObj = new object();
        /// <inheritdoc />
        public override void PostConfigureServices(ServiceConfigurationContext context)
        {
            var services = context.Services;

            // AutoMapper
            var config = new MapperConfigurationExpression();
            var actions = services.GetObject<AutoMapperConfig>()?.MapActions;
            if (actions != null)
            {
                foreach (var action in actions)
                {
                    action(config);
                }
            }
            lock (_autoMapperSyncObj)
            {
                if (!_staticMapperInitialized)
                {
                    _staticMapperInitialized = true;
                    Mapper.Initialize(config);
                }
            }
            services.AddSingleton(Mapper.Instance);
            services.RemoveAll<IObjectAccessor<AutoMapperConfig>>();
        }


        public override void ConfigureServices(ServiceConfigurationContext context)
    {

            var services = context.Services;
            var config = services.GetConfiguration();
            context.Services.AddSingleton<ICancellationTokenProvider>(NullCancellationTokenProvider.Instance);
                Configure<AbpUnitOfWorkDefaultOptions>(options =>
            {
                options.IsTransactional = true;
                options.IsolationLevel = IsolationLevel.ReadCommitted;
            });

                Configure<AbpDbConnectionOptions>(config);
                Configure<AbpDataFilterOptions>(options =>
                {
                    options.DefaultStates[typeof(ISoftDelete)] = new DataFilterState(true);
                });
            context.Services.AddSingleton(typeof(IDataFilter<>), typeof(DataFilter<>));

            // 配置AutoMapper
            services.ConfigMapper(CreateDtoMappings);
            services.ConfigMapper(ConfigMapFromAttribute);
        }
        /// <summary>
        /// 创建 Dto 对象映射
        /// </summary>
        private void CreateDtoMappings(IMapperConfigurationExpression config)
        {
            // 配置默认不检查所有属性映射
            config.ForAllMaps((t, c) => c.ValidateMemberList(MemberList.None));

            config.CreateMap<Enum, DisplayItem>()
                .ConvertUsing(p => new DisplayItem
                {
                    Id = p,
                    Name = p.DisplayName(),
                    Value = p.ToString("G"),
                    ShortName = p.DisplayShortName(),
                    Description = p.DisplayDescription(),
                    GroupName = p.DisplayGroupName(),
                    Order = p.DisplayOrder(),
                    Type = p.DisplayPrompt(),
                });

            //config.CreateMap<TokenResponse, TokenResult>(MemberList.None);
        }
        /// <summary>
        /// 配置 <see cref="MapFromAttribute"/> 映射
        /// </summary>
        private static void ConfigMapFromAttribute(IMapperConfigurationExpression config)
        {
            config.ForAllMaps((t, c) =>
            {
                var map = typeof(MemberConfigurationExpression<object, object, object>)
                    .GetMethod("MapFromUntyped", BindingFlags.NonPublic | BindingFlags.Instance);
                c.ForAllOtherMembers(memberOptions =>
                {
                    var member = memberOptions.DestinationMember;
                    var att = member.GetCustomAttribute<MapFromAttribute>();
                    if (att?.PropertyPath?.Length > 0)
                    {
                        var source = Expression.Parameter(t.SourceType);
                        Expression body = source;
                        foreach (var path in att.PropertyPath)
                        {
                            body = Expression.PropertyOrField(body, path);
                        }

                        var mapFromExpression = Expression.Lambda(body, source);

                        map.Invoke(memberOptions, new object[] { mapFromExpression });
                    }
                });
            });
        }
    }
   
}

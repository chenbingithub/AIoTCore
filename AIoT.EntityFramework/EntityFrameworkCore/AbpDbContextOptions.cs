using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Volo.Abp;

namespace AIoT.EntityFramework.EntityFrameworkCore
{
    public class AbpDbContextOptions
    {
        public List<Action<DependencyInjection.AbpDbContextConfigurationContext>> DefaultPreConfigureActions { get; set; }

        public Action<DependencyInjection.AbpDbContextConfigurationContext> DefaultConfigureAction { get; set; }

        public Dictionary<Type, List<object>> PreConfigureActions { get; set; }

        public Dictionary<Type, object> ConfigureActions { get; set; }

        public AbpDbContextOptions()
        {
            DefaultPreConfigureActions = new List<Action<DependencyInjection.AbpDbContextConfigurationContext>>();
            PreConfigureActions = new Dictionary<Type, List<object>>();
            ConfigureActions = new Dictionary<Type, object>();
        }

        public void PreConfigure([NotNull] Action<DependencyInjection.AbpDbContextConfigurationContext> action)
        {
            Check.NotNull(action, nameof(action));

            DefaultPreConfigureActions.Add(action);
        }

        public void Configure([NotNull] Action<DependencyInjection.AbpDbContextConfigurationContext> action)
        {
            Check.NotNull(action, nameof(action));

            DefaultConfigureAction = action;
        }

        public void PreConfigure<TDbContext>([NotNull] Action<DependencyInjection.AbpDbContextConfigurationContext<TDbContext>> action)
            where TDbContext : AbpDbContext<TDbContext>
        {
            Check.NotNull(action, nameof(action));

            var actions = PreConfigureActions.GetOrDefault(typeof(TDbContext));
            if (actions == null)
            {
                PreConfigureActions[typeof(TDbContext)] = actions = new List<object>();
            }

            actions.Add(action);
        }

        public void Configure<TDbContext>([NotNull] Action<DependencyInjection.AbpDbContextConfigurationContext<TDbContext>> action) 
            where TDbContext : AbpDbContext<TDbContext>
        {
            Check.NotNull(action, nameof(action));

            ConfigureActions[typeof(TDbContext)] = action;
        }
    }
}
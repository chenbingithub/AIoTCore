using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Volo.Abp;

namespace AIoT.EntityFramework.EntityFrameworkCore
{
    public class AbpDbContextOptions
    {
        internal List<Action<EntityFramework.EntityFrameworkCore.AbpDbContextConfigurationContext>> DefaultPreConfigureActions { get; set; }

        internal Action<EntityFramework.EntityFrameworkCore.AbpDbContextConfigurationContext> DefaultConfigureAction { get; set; }

        internal Dictionary<Type, List<object>> PreConfigureActions { get; set; }

        internal Dictionary<Type, object> ConfigureActions { get; set; }

        public AbpDbContextOptions()
        {
            DefaultPreConfigureActions = new List<Action<EntityFramework.EntityFrameworkCore.AbpDbContextConfigurationContext>>();
            PreConfigureActions = new Dictionary<Type, List<object>>();
            ConfigureActions = new Dictionary<Type, object>();
        }

        public void PreConfigure([NotNull] Action<EntityFramework.EntityFrameworkCore.AbpDbContextConfigurationContext> action)
        {
            Check.NotNull(action, nameof(action));

            DefaultPreConfigureActions.Add(action);
        }

        public void Configure([NotNull] Action<EntityFramework.EntityFrameworkCore.AbpDbContextConfigurationContext> action)
        {
            Check.NotNull(action, nameof(action));

            DefaultConfigureAction = action;
        }

        public void PreConfigure<TDbContext>([NotNull] Action<EntityFramework.EntityFrameworkCore.AbpDbContextConfigurationContext<TDbContext>> action)
            where TDbContext : EntityFramework.EntityFrameworkCore.AbpDbContext<TDbContext>
        {
            Check.NotNull(action, nameof(action));

            var actions = PreConfigureActions.GetValueOrDefault(typeof(TDbContext));
            if (actions == null)
            {
                PreConfigureActions[typeof(TDbContext)] = actions = new List<object>();
            }

            actions.Add(action);
        }

        public void Configure<TDbContext>([NotNull] Action<EntityFramework.EntityFrameworkCore.AbpDbContextConfigurationContext<TDbContext>> action) 
            where TDbContext : EntityFramework.EntityFrameworkCore.AbpDbContext<TDbContext>
        {
            Check.NotNull(action, nameof(action));

            ConfigureActions[typeof(TDbContext)] = action;
        }
    }
}
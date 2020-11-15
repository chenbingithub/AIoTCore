using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AIoT.Core.Entities;
using AIoT.Core.EntityFrameworkCore;
using AIoT.Core.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Volo.Abp.Reflection;

namespace AIoT.Core.Extensions
{
    public static class RepositoryServiceCollectionExtensions
    {
        public static IServiceCollection AddAbpDbContext<TDbContext>(
            this IServiceCollection services,
            Action<AbpDbContextConfigurationContext<TDbContext>> options = null
            ,bool registerRepository = false)
            where TDbContext : AbpDbContext<TDbContext>
        {
            

            services.TryAddTransient(DbContextOptionsFactory.Create<TDbContext>);
            services.AddDbContext<TDbContext>(ServiceLifetime.Transient, ServiceLifetime.Transient);

            services.Configure<AbpDbContextOptions>(p => { p.Configure(options); });
            if (registerRepository)
            {
                services.AddRepositoryWithDbSet<TDbContext>();
            }

            return services;
        }

        /// <summary>
        /// 注册泛型仓储接口,指定的<typeparamref name="TDbContext"/>定义的<see cref="DbSet{TEntity}"/>实体都会注册泛型仓储. <br/>
        /// <see cref="IRepository{TEntity}"/> <see cref="IRepository{TEntity, TKey}"/><br/>
        /// <see cref="IReadRepository{TEntity}"/> <see cref="IReadRepository{TEntity, TKey}"/><br/>
        /// <see cref="IWriteRepository{TEntity}"/> <see cref="IWriteRepository{TEntity, TKey}"/><br/>
        /// </summary>
        /// <typeparam name="TDbContext">指定DbContext类型</typeparam>
        /// <param name="service"></param>
        /// <param name="repositoryImplType">default = typeof(<see cref="EfRepository{TDbContext,TEntity}"/>)</param>
        /// <param name="repositoryByKeyImplType">default = typeof(<see cref="EfRepository{TDbContext,TEntity,TKey}"/>)</param>
        public static IServiceCollection AddRepositoryWithDbSet<TDbContext>(this IServiceCollection service,
            Type repositoryImplType = default, Type repositoryByKeyImplType = default)
            where TDbContext : DbContext
        {
            var dbContextType = typeof(TDbContext);
            var entityTypes = DbContextHelper.GetEntityTypes(typeof(TDbContext));
            repositoryImplType ??= typeof(EfRepository<,>);
            repositoryByKeyImplType ??= typeof(EfRepository<,,>);

            var entityType = typeof(IEntity);
            var entityByKeyType = typeof(IEntity<>);

            foreach (var type in entityTypes)
            {
                if (entityType.IsAssignableFrom(type))
                {
                    var implType = repositoryImplType.MakeGenericType(dbContextType, type);
                    foreach (var p in DbContextHelper.GetRepositoryType(type))
                    {
                        service.TryAddTransient(p, implType);
                    }
                }

                var keyType = type.GetGenericInterfaceArguments(entityByKeyType)?.FirstOrDefault();
                if (keyType != null)
                {
                    var implType = repositoryByKeyImplType.MakeGenericType(dbContextType, type, keyType);
                    foreach (var p in DbContextHelper.GetRepositoryType(type, keyType))
                    {
                        service.TryAddTransient(p, implType);
                    }
                }
            }

            return service;
        }
        public static Type[] GetGenericInterfaceArguments(this Type type, Type generic)
        {
            if (type == null || generic == null)
            {
                throw new ArgumentNullException();
            }

            if (!generic.IsInterface)
            {
                throw new ArgumentException(nameof(generic) + " is must be interface.");
            }

            if (!generic.IsGenericTypeDefinition)
            {
                throw new ArgumentException(nameof(generic) + " is must be generic type definition.");
            }

            if (!generic.IsGenericType)
            {
                return Type.EmptyTypes;
            }

            if (type == generic)
                return Type.EmptyTypes;
            while (type != null)
            {
                Type t;
                if (type.IsGenericType && (t = type.GetInterfaces()
                    .FirstOrDefault(o => o.IsGenericType && o.GetGenericTypeDefinition() == generic)) != null)
                    return t.GetGenericArguments();
                type = type.BaseType;
            }

            return Type.EmptyTypes;
        }
    }
    internal static class DbContextHelper
    {
        public static IEnumerable<Type> GetEntityTypes(Type dbContextType)
        {
            return
                from property in dbContextType.GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                where
                    ReflectionHelper.IsAssignableToGenericType(property.PropertyType, typeof(DbSet<>)) &&
                    typeof(IEntity).IsAssignableFrom(property.PropertyType.GenericTypeArguments[0])
                select property.PropertyType.GenericTypeArguments[0];
        }

        public static IEnumerable<Type> GetRepositoryType(Type entityType)
        {
            yield return typeof(IRepository<>).MakeGenericType(entityType);
            yield return typeof(IReadRepository<>).MakeGenericType(entityType);
            yield return typeof(IWriteRepository<>).MakeGenericType(entityType);
        }

        public static IEnumerable<Type> GetRepositoryType(Type entityType, Type primaryKeyType)
        {
            yield return typeof(IRepository<,>).MakeGenericType(entityType, primaryKeyType);
            yield return typeof(IReadRepository<,>).MakeGenericType(entityType, primaryKeyType);
            yield return typeof(IWriteRepository<,>).MakeGenericType(entityType, primaryKeyType);
        }
    }
}

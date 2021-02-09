using System;
using System.Collections.Generic;
using AIoT.Core.Entities;
using AIoT.Core.Repository;
using AIoT.EntityFramework.EfCore;
using AIoT.EntityFramework.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AIoT.EntityFramework.DependencyInjection
{
    public static class ServiceCollectionRepositoryExtensions
    {
        public static IServiceCollection AddEfCoreRepository<TDbContext>(this IServiceCollection service) where TDbContext : AbpDbContext<TDbContext>
        {
            var dbContextType = typeof(TDbContext);
            var entityTypes = DbContextHelper.GetEntityTypes(dbContextType);
            foreach (var entityType in entityTypes)
            {
                AddDefaultRepository(service,entityType, GetDefaultRepositoryImplementationType(dbContextType, entityType));
            }

            return service;
        }
        /// <summary>
        /// 获取仓储实现
        /// </summary>
        /// <param name="dbContextType"></param>
        /// <param name="entityType"></param>
        /// <returns></returns>
        private static Type GetDefaultRepositoryImplementationType(Type dbContextType,Type entityType)
        {
            var primaryKeyType = EntityHelper.FindPrimaryKeyType(entityType);

            if (primaryKeyType == null)
            {
                return typeof(EfCoreRepository<,>).MakeGenericType(dbContextType, entityType);
            }

            return typeof(EfCoreRepository<,,>).MakeGenericType(dbContextType, entityType, primaryKeyType);
        }
   

        
        /// <summary>
        /// 注入仓储
        /// </summary>
        /// <param name="services"></param>
        /// <param name="entityType"></param>
        /// <param name="repositoryImplementationType"></param>
        /// <returns></returns>
        private static IServiceCollection AddDefaultRepository(this IServiceCollection services, Type entityType, Type repositoryImplementationType)
        {
            //IReadOnlyBasicRepository<TEntity>
            var readOnlyBasicRepositoryInterface = typeof(IReadOnlyBasicRepository<>).MakeGenericType(entityType);
            if (readOnlyBasicRepositoryInterface.IsAssignableFrom(repositoryImplementationType))
            {
                services.TryAddTransient(readOnlyBasicRepositoryInterface, repositoryImplementationType);

                //IReadOnlyRepository<TEntity>
                var readOnlyRepositoryInterface = typeof(IReadOnlyRepository<>).MakeGenericType(entityType);
                if (readOnlyRepositoryInterface.IsAssignableFrom(repositoryImplementationType))
                {
                    services.TryAddTransient(readOnlyRepositoryInterface, repositoryImplementationType);
                }

                //IBasicRepository<TEntity>
                var basicRepositoryInterface = typeof(IBasicRepository<>).MakeGenericType(entityType);
                if (basicRepositoryInterface.IsAssignableFrom(repositoryImplementationType))
                {
                    services.TryAddTransient(basicRepositoryInterface, repositoryImplementationType);

                    //IRepository<TEntity>
                    var repositoryInterface = typeof(IRepository<>).MakeGenericType(entityType);
                    if (repositoryInterface.IsAssignableFrom(repositoryImplementationType))
                    {
                        services.TryAddTransient(repositoryInterface, repositoryImplementationType);
                    }
                }
            }

            var primaryKeyType = EntityHelper.FindPrimaryKeyType(entityType);
            if (primaryKeyType != null)
            {
                //IReadOnlyBasicRepository<TEntity, TKey>
                var readOnlyBasicRepositoryInterfaceWithPk = typeof(IReadOnlyBasicRepository<,>).MakeGenericType(entityType, primaryKeyType);
                if (readOnlyBasicRepositoryInterfaceWithPk.IsAssignableFrom(repositoryImplementationType))
                {
                    services.TryAddTransient(readOnlyBasicRepositoryInterfaceWithPk, repositoryImplementationType);

                    //IReadOnlyRepository<TEntity, TKey>
                    var readOnlyRepositoryInterfaceWithPk = typeof(IReadOnlyRepository<,>).MakeGenericType(entityType, primaryKeyType);
                    if (readOnlyRepositoryInterfaceWithPk.IsAssignableFrom(repositoryImplementationType))
                    {
                        services.TryAddTransient(readOnlyRepositoryInterfaceWithPk, repositoryImplementationType);
                    }

                    //IBasicRepository<TEntity, TKey>
                    var basicRepositoryInterfaceWithPk = typeof(IBasicRepository<,>).MakeGenericType(entityType, primaryKeyType);
                    if (basicRepositoryInterfaceWithPk.IsAssignableFrom(repositoryImplementationType))
                    {
                        services.TryAddTransient(basicRepositoryInterfaceWithPk, repositoryImplementationType);

                        //IRepository<TEntity, TKey>
                        var repositoryInterfaceWithPk = typeof(IRepository<,>).MakeGenericType(entityType, primaryKeyType);
                        if (repositoryInterfaceWithPk.IsAssignableFrom(repositoryImplementationType))
                        {
                            services.TryAddTransient(repositoryInterfaceWithPk, repositoryImplementationType);
                        }
                    }
                }
            }

            return services;
        }
    }
}
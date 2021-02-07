using System;
using AIoT.Core.Entities;
using AIoT.Core.Repository;
using AIoT.EntityFramework.Domain.Repositories.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AIoT.EntityFramework.Domain.Repositories
{
    public static class EfCoreRepositoryExtensions
    {
        public static DbContext GetDbContext<TEntity>(this IReadOnlyBasicRepository<TEntity> repository)
            where TEntity : class, IEntity
        {
            return repository.ToEfCoreRepository().DbContext;
        }

        public static DbSet<TEntity> GetDbSet<TEntity>(this IReadOnlyBasicRepository<TEntity> repository)
            where TEntity : class, IEntity
        {
            return repository.ToEfCoreRepository().DbSet;
        }

        public static IEfCoreRepository<TEntity> ToEfCoreRepository<TEntity>(this IReadOnlyBasicRepository<TEntity> repository)
            where TEntity : class, IEntity
        {
            if (repository is IEfCoreRepository<TEntity> efCoreRepository)
            {
                return efCoreRepository;
            }

            throw new ArgumentException("Given repository does not implement " + typeof(IEfCoreRepository<TEntity>).AssemblyQualifiedName, nameof(repository));
        }
    }
}

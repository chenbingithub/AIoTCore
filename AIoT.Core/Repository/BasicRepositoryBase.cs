using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AIoT.Core.Entities;
using AIoT.Core.Threading;
using AIoT.Core.Uow;
using Volo.Abp.DependencyInjection;

namespace AIoT.Core.Repository
{
    public abstract class BasicRepositoryBase<TEntity> : 
        IBasicRepository<TEntity>, 
        IServiceProviderAccessor,
        ITransientDependency
        where TEntity : class, IEntity
    {
        public IServiceProvider ServiceProvider { get; set; }

        public ICancellationTokenProvider CancellationTokenProvider { get; set; }

        protected BasicRepositoryBase()
        {
            CancellationTokenProvider = NullCancellationTokenProvider.Instance;
        }

        public abstract Task<TEntity> InsertAsync(TEntity entity, bool autoSave = false, CancellationToken cancellationToken = default);

        public abstract Task<TEntity> UpdateAsync(TEntity entity, bool autoSave = false, CancellationToken cancellationToken = default);

        public abstract Task DeleteAsync(TEntity entity, bool autoSave = false, CancellationToken cancellationToken = default);

        public abstract Task<List<TEntity>> GetListAsync(bool includeDetails = false, CancellationToken cancellationToken = default);

        public abstract Task<long> GetCountAsync(CancellationToken cancellationToken = default);

        public abstract Task<List<TEntity>> GetPagedListAsync(int skipCount, int maxResultCount, string sorting, bool includeDetails = false, CancellationToken cancellationToken = default);

        protected virtual CancellationToken GetCancellationToken(CancellationToken preferredValue = default)
        {
            return CancellationTokenProvider.FallbackToProvider(preferredValue);
        }
    }

    public abstract class BasicRepositoryBase<TEntity, TKey> : BasicRepositoryBase<TEntity>, IBasicRepository<TEntity, TKey>
        where TEntity : class, IEntity<TKey>
    {
        public virtual async Task<TEntity> GetAsync(TKey id, bool includeDetails = true, CancellationToken cancellationToken = default)
        {
            var entity = await FindAsync(id, includeDetails, cancellationToken);

            if (entity == null)
            {
                throw new Exception($"There is no such an entity. Entity type: {typeof(TEntity).FullName}, id: {id}");
            }

            return entity;
        }

        public abstract Task<TEntity> FindAsync(TKey id, bool includeDetails = true, CancellationToken cancellationToken = default);
        
        public virtual async Task DeleteAsync(TKey id, bool autoSave = false, CancellationToken cancellationToken = default)
        {
            var entity = await FindAsync(id, cancellationToken: cancellationToken);
            if (entity == null)
            {
                return;
            }

            await DeleteAsync(entity, autoSave, cancellationToken);
        }
    }
}

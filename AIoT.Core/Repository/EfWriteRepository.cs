using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AIoT.Core.Entities;
using AIoT.Core.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AIoT.Core.Repository
{
    /// <summary>
    /// 写库仓储
    /// </summary>
    public class EfWriteRepository<TDbContext, TEntity> :
        EfReadRepository<TDbContext, TEntity>,
        IWriteRepository<TEntity>
        where TDbContext : DbContext
        where TEntity : class, IEntity
    {
        #region 构造函数、私有字段


        /// <inheritdoc />
        public EfWriteRepository(IDbContextProvider dbContextProvider) : base(dbContextProvider)
        {
        }

        #endregion

    

        #region IWriteRepository

        /// <inheritdoc />
        public async Task<TEntity> GetAsync(params object[] keys)
        {
            return await Table.FindAsync(keys, CancellationToken);
        }

        /// <inheritdoc />
        public async Task<TEntity> FirstOrDefaultAsync(params object[] keys)
        {
            return await Table.FindAsync(keys, CancellationToken);
        }

        /// <inheritdoc />
        public virtual IQueryable<TEntity> Including(params Expression<Func<TEntity, object>>[] propertySelectors)
        {
            IQueryable<TEntity> query = Table;

            if (!(propertySelectors==null|| propertySelectors.Length<=0))
            {
                foreach (var propertySelector in propertySelectors)
                {
                    query = query.Include(propertySelector);
                }
            }

            return query;
        }

        /// <inheritdoc />
        public virtual async Task<List<TEntity>> GetAllListAsync()
        {
            return await Table.ToListAsync(CancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task<List<TEntity>> GetAllListAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await Table.Where(predicate).ToListAsync(CancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task<TEntity> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await Table.SingleAsync(predicate, CancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await Table.FirstOrDefaultAsync(predicate, CancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task<TEntity> InsertAsync(TEntity entity)
        {
            await Table.AddAsync(entity, CancellationToken);

            await SaveChangesAsync(CancellationToken);

            return entity;
        }

        /// <inheritdoc />
        public async Task InsertRangeAsync(IEnumerable<TEntity> entities)
        {
            await Table.AddRangeAsync(entities, CancellationToken);

            await SaveChangesAsync(CancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task<TEntity> UpdateAsync(TEntity entity)
        {
            var entry = Context.Entry(entity);
            if (entry.State != EntityState.Modified && entry.State != EntityState.Unchanged)
                entry.State = EntityState.Modified;

            await SaveChangesAsync(CancellationToken);

            return entity;
        }

        /// <inheritdoc />
        public async Task DeleteAsync(params object[] keys)
        {
            var entity = await Table.FindAsync(keys, CancellationToken);
            await DeleteAsync(entity);
        }

        /// <inheritdoc />
        public virtual async Task DeleteAsync(TEntity entity)
        {
            Table.Remove(entity);
            await SaveChangesAsync(CancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task DeleteAsync(Expression<Func<TEntity, bool>> predicate)
        {
            var entities = await Table.Where(predicate).ToListAsync();
            foreach (var entity in entities)
            {
                await DeleteAsync(entity);
            }
        }

        /// <inheritdoc />
        public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await Context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// 附加到DbContext
        /// </summary>
        protected void AttachIfNot(TEntity entity)
        {
            if (!Table.Local.Contains(entity))
            {
                Table.Attach(entity);
            }
        }

        #endregion
    }

    /// <inheritdoc cref="EfWriteRepository{TDbContext,TEntity}" />
    public class EfWriteRepository<TDbContext, TEntity, TKey> :
        EfWriteRepository<TDbContext, TEntity>,
        IWriteRepository<TEntity, TKey>
        where TDbContext : DbContext
        where TEntity : class, IEntity<TKey>
    {
        /// <inheritdoc />
        public EfWriteRepository(IDbContextProvider dbContextProvider) : base(dbContextProvider)
        {
        }

        /// <inheritdoc />
        public virtual async Task<TEntity> GetAsync(TKey key)
        {
            var entity = await Table.FindAsync(new object[] { key }, CancellationToken);

            return entity;
        }

        /// <inheritdoc />
        public virtual async Task<TEntity> FirstOrDefaultAsync(TKey key)
        {
            return await Table.FindAsync(new object[] { key }, CancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task DeleteAsync(TKey key)
        {
            var entity = await Table.FindAsync(new object[] { key }, CancellationToken);
            await DeleteAsync(entity);
        }

        /// <inheritdoc />
        public Expression<Func<TEntity, bool>> GetKeyExpression(TKey key)
        {
            return CreateEqualityExpressionForKeys<TEntity, TKey>(key);
        }

        

        /// <inheritdoc />
        //public async Task<TDto> FirstOrDefaultAsync<TDto>(TKey key)
        //{
        //    var filter = CreateEqualityExpressionForKeys<TEntity, TKey>(key);

        //    return await Table.Where(filter).ProjectTo<TDto>().FirstOrDefaultAsync(CancellationToken);
        //}
    }
}

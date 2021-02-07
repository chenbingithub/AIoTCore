using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AIoT.Core.Uow;
using AIoT.EntityFramework.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace AIoT.EntityFramework.Uow.EntityFrameworkCore
{
    public class EfCoreTransactionApi : ITransactionApi, ISupportsRollback
    {
        public IDbContextTransaction DbContextTransaction { get; }
        public IEfCoreDbContext StarterDbContext { get; }
        public List<IEfCoreDbContext> AttendedDbContexts { get; }

        public EfCoreTransactionApi(IDbContextTransaction dbContextTransaction, IEfCoreDbContext starterDbContext)
        {
            DbContextTransaction = dbContextTransaction;
            StarterDbContext = starterDbContext;
            AttendedDbContexts = new List<IEfCoreDbContext>();
        }

        public Task CommitAsync()
        {
            Commit();
            return Task.CompletedTask;
        }
        
        public void Commit()
        {
            DbContextTransaction.Commit();

            foreach (var dbContext in AttendedDbContexts)
            {
                if (dbContext.As<DbContext>().HasRelationalTransactionManager())
                {
                    continue; //Relational databases use the shared transaction
                }

                dbContext.Database.CommitTransaction();
            }
        }

        public void Dispose()
        {
            DbContextTransaction.Dispose();
        }

        public void Rollback()
        {
            DbContextTransaction.Rollback();
        }

        public Task RollbackAsync(CancellationToken cancellationToken)
        {
            DbContextTransaction.Rollback();
            return Task.CompletedTask;
        }
    }
}
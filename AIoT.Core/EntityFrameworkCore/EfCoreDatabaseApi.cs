using System.Threading;
using System.Threading.Tasks;
using AIoT.Core.Uow;
using Microsoft.EntityFrameworkCore;

namespace AIoT.Core.EntityFrameworkCore
{
    public class EfCoreDatabaseApi<TDbContext> : IDatabaseApi, ISupportsSavingChanges
        where TDbContext : DbContext
    {
        public TDbContext DbContext { get; }

        public EfCoreDatabaseApi(TDbContext dbContext)
        {
            DbContext = dbContext;
        }
        
        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return DbContext.SaveChangesAsync(cancellationToken);
        }

        public void SaveChanges()
        {
            DbContext.SaveChanges();
        }
    }
}
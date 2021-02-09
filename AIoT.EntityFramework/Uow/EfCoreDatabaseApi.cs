using System.Threading;
using System.Threading.Tasks;
using AIoT.Core.Uow;
using AIoT.EntityFramework.EfCore;

namespace AIoT.EntityFramework.Uow
{
    public class EfCoreDatabaseApi<TDbContext> : IDatabaseApi, ISupportsSavingChanges
        where TDbContext : IEfCoreDbContext
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
    }
}
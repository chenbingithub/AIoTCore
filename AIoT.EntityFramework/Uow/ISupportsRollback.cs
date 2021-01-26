using System.Threading;
using System.Threading.Tasks;

namespace AIoT.EntityFramework.Uow
{
    public interface ISupportsRollback
    {
        void Rollback();

        Task RollbackAsync(CancellationToken cancellationToken);
    }
}
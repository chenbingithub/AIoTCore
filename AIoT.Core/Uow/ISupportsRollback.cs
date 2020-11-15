using System.Threading;
using System.Threading.Tasks;

namespace AIoT.Core.Uow
{
    public interface ISupportsRollback
    {
        void Rollback();

        Task RollbackAsync(CancellationToken cancellationToken);
    }
}
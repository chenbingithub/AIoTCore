using System.Threading;
using System.Threading.Tasks;

namespace AIoT.EntityFramework.Uow
{
    public interface ISupportsSavingChanges
    {
        void SaveChanges();

        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
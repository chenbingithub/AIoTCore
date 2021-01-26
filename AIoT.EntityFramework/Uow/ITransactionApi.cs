using System;
using System.Threading.Tasks;

namespace AIoT.EntityFramework.Uow
{
    public interface ITransactionApi : IDisposable
    {
        void Commit();

        Task CommitAsync();
    }
}
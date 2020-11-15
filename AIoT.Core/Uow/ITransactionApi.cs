using System;
using System.Threading.Tasks;

namespace AIoT.Core.Uow
{
    public interface ITransactionApi : IDisposable
    {
        void Commit();

        Task CommitAsync();
    }
}
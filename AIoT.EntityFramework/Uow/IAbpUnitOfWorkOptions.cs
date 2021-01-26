using System;
using System.Data;

namespace AIoT.EntityFramework.Uow
{
    public interface IAbpUnitOfWorkOptions
    {
        bool IsTransactional { get; }

        IsolationLevel? IsolationLevel { get; }

        TimeSpan? Timeout { get; }
    }
}
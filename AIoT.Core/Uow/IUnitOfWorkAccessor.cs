using JetBrains.Annotations;

namespace AIoT.Core.Uow
{
    public interface IUnitOfWorkAccessor
    {
        [CanBeNull]
        IUnitOfWork UnitOfWork { get; }

        void SetUnitOfWork([CanBeNull] IUnitOfWork unitOfWork);
    }
}
using JetBrains.Annotations;

namespace AIoT.EntityFramework.Uow
{
    public interface IUnitOfWorkManager
    {
        [CanBeNull]
        IUnitOfWork Current { get; }

        [NotNull]
        IUnitOfWork Begin([NotNull] EntityFramework.Uow.AbpUnitOfWorkOptions options, bool requiresNew = false);

        [NotNull]
        IUnitOfWork Reserve([NotNull] string reservationName, bool requiresNew = false);

        void BeginReserved([NotNull] string reservationName, [NotNull] EntityFramework.Uow.AbpUnitOfWorkOptions options);

        bool TryBeginReserved([NotNull] string reservationName, [NotNull] EntityFramework.Uow.AbpUnitOfWorkOptions options);
    }
}
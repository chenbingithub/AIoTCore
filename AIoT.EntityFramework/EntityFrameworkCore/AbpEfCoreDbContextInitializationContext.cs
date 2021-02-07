using AIoT.Core.Uow;

namespace AIoT.EntityFramework.EntityFrameworkCore
{
    public class AbpEfCoreDbContextInitializationContext
    {
        public IUnitOfWork UnitOfWork { get; }

        public AbpEfCoreDbContextInitializationContext(IUnitOfWork unitOfWork)
        {
            UnitOfWork = unitOfWork;
        }
    }
}
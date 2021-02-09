using AIoT.Core.Uow;

namespace AIoT.EntityFramework.EfCore
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
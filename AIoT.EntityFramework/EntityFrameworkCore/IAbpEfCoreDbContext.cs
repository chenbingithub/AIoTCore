using AIoT.EntityFramework.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace AIoT.EntityFramework.EntityFrameworkCore
{
    public interface IAbpEfCoreDbContext : IEfCoreDbContext
    {
        void Initialize(AbpEfCoreDbContextInitializationContext initializationContext);
    }
}
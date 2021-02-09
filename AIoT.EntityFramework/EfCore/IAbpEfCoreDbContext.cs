
namespace AIoT.EntityFramework.EfCore
{
    public interface IAbpEfCoreDbContext : IEfCoreDbContext
    {
        void Initialize(AbpEfCoreDbContextInitializationContext initializationContext);
    }
}
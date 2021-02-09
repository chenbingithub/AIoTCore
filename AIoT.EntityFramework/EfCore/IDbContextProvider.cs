
namespace AIoT.EntityFramework.EfCore
{
    public interface IDbContextProvider<out TDbContext>
        where TDbContext : IEfCoreDbContext
    {
        TDbContext GetDbContext();
    }
}
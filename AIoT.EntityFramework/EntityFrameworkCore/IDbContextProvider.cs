using AIoT.EntityFramework.EntityFrameworkCore;

namespace AIoT.EntityFramework.EntityFrameworkCore
{
    public interface IDbContextProvider<out TDbContext>
        where TDbContext : IEfCoreDbContext
    {
        TDbContext GetDbContext();
    }
}
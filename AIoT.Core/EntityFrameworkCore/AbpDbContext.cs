using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.DependencyInjection;

namespace AIoT.Core.EntityFrameworkCore
{
    
    public class AbpDbContext<TDbContext> : DbContext,ITransientDependency
        where TDbContext : DbContext
    {
        public AbpDbContext(DbContextOptions<TDbContext> options)
            : base(options)
        {

        }
       
    }


}

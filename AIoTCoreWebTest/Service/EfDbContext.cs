using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using AIoT.Core.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AIoTCoreWebTest.Service
{
    public class EfDbContext : AbpDbContext<EfDbContext>
    {
        public EfDbContext(DbContextOptions<EfDbContext> options) : base(options)
        {
        }
        public virtual DbSet<Product> Products { get; set; }
    }

   
    [Table("product")]
    public partial class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }

    }
}

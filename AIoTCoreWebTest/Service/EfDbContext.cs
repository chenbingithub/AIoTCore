using System;
using System.Collections.Generic;
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
    }
}

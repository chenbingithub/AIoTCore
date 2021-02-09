using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using AIoT.Core.Entities.Auditing;
using AIoT.EntityFramework.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AIoTCoreWebTest.Service
{
    public class MyDbContext: AbpDbContext<MyDbContext>
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {
        }
        public virtual DbSet<Role> Roles { get; set; }
    }

    [Table("T_Sys_Role")]
    public class Role : FullAuditedEntityName<string, string>
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        [Required]
        [StringLength(50)]
        public string Code { get; set; }
        [StringLength(50)]
        public string Description { get; set; }
    }
}

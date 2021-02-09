using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AIoT.Core.Enums;
using AIoT.Core.Service;
using AIoT.Core.Uow;
using AIoTCoreWebTest.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AIoTCoreWebTest.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class HomeController : ControllerBase
    {
        private IServiceProvider _serviceProvider;

        public HomeController(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            //_context = context;
        }

        public async Task<string> Index()
        {
            try
            {
                
                
                
                
                
                
                var db = _serviceProvider.GetService<MyDbContext>();
                var r = await db.Roles.Where(u=>true).ToListAsync();
                foreach (var role in r)
                {
                    Console.WriteLine(role.Name);
                }

                return $"ok{r.Count}";
            }
            catch (Exception e)
            {
                
                Console.WriteLine(e);
                throw;
            }
          
        }
    }
}

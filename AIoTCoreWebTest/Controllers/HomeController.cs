using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AIoT.Core.EntityFrameworkCore;
using AIoT.Core.Enums;
using AIoT.Core.Service;
using AIoT.Core.Uow;
using AIoTCoreWebTest.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AIoTCoreWebTest.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class HomeController : ControllerBase, IUnitOfWorkEnabled
    {
        private readonly IMyService _myService;
        private readonly IDbContextProvider _contextProvider;

        public HomeController( IMyService myService, IDbContextProvider contextProvider)
        {
            _myService = myService;
            _contextProvider = contextProvider;
        }

        [HttpGet]
        public string Index()
        {
           return _myService.Test();
        }

        [HttpGet]
        public string Index1()
        {
           var db= _contextProvider.GetDbContext<EfDbContext>(EfCoreDatabaseProvider.MySql);
          var ds= db.Products.ToList();
            return "ok";
        }
        [HttpGet]
        public string Index2()
        {
            var db = _contextProvider.GetDbContext<EfDbContext>(EfCoreDatabaseProvider.MySql);
            db.Products.Add(new AIoT.Core.EntityFrameworkCore.Product()
            {
                Id = 1,
                Name = "3"

            });
            db.SaveChanges();
            return "ok";
        }
    }
}

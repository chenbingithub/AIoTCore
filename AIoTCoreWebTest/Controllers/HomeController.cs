using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AIoT.Core.EntityFrameworkCore;
using AIoT.Core.Service;
using AIoT.Core.Uow;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AIoTCoreWebTest.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class HomeController : ControllerBase, IUnitOfWorkEnabled
    {
        private readonly IMyService _myService;

        public HomeController( IMyService myService)
        {
            _myService = myService;
        }

        [HttpGet]
        public string Index()
        {
           return _myService.Test();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AIoT.Core.Enums;
using AIoT.Core.Service;
using AIoT.Core.Uow;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AIoTCoreWebTest.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class HomeController : ControllerBase, IUnitOfWorkEnabled
    {

    }
}

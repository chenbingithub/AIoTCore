using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AIoT.Core.Dto;
using AIoT.Core.Enums;
using AIoT.Core.Repository;
using AIoT.Core.Service;
using AIoT.Core.Uow;
using AIoT.Module.SysManager.Application;
using AIoT.RedisCache.Cache;
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
        private IRoleService roleService;
        private IRoleCache _roleCache;
        private IRoleListCache _roleListCache;
        private ICacheManager _cacheManager;
        private readonly IRepository<Role, string> _roleRepository;


        public HomeController(IServiceProvider serviceProvider, IRoleService roleService, IRoleCache roleCache, ICacheManager cacheManager, IRepository<Role, string> roleRepository, IRoleListCache roleListCache)
        {
            _serviceProvider = serviceProvider;
            this.roleService = roleService;
            _roleCache = roleCache;
            _cacheManager = cacheManager;
            _roleRepository = roleRepository;
            _roleListCache = roleListCache;
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
        public async Task<string> Index1()
        {
            try
            {


                var db = _serviceProvider.GetService<IRepository<Role>>();

                var data=await db.FirstOrDefaultAsync<Role,RoleDto>(u=>u.Code== "string");
                return $"ok";
            }
            catch (Exception e)
            {

                Console.WriteLine(e);
                throw;
            }

        }
        public async Task<Result> Index2()
        {
            return await roleService.AddAsync(new RoleDto
            {
                Id=Guid.NewGuid().ToString("D"),
                Name = "共和国dsaf",
                Code = " sdf"
            });
            
        }
        public async Task<Result> Index3()
        {
            return await roleService.UpdateAsync(new RoleDto
            {
                Id = "3c9310b7-a6d4-42cb-9cdd-adf5718a98b9",
                Name = "qqqqqq",
                Code = " qqqq"
            });
        }
        public async Task<Result> Index4()
        {
            return await roleService.DeleteAsync("3c9310b7-a6d4-42cb-9cdd-adf5718a98b9");
        }
        public async Task<RoleCacheItem> Index5()
        {
            return await _roleCache.GetAsync("3c9310b7-a6d4-42cb-9cdd-adf5718a98b9");
        }
        public async Task<RoleCacheItem> Index6()
        {
            return await _cacheManager.GetCache<RoleCacheItem>().GetOrAddAsync("3c9310b7-a6d4-42cb-9cdd-adf5718a98b9",
                () => _roleRepository.FirstOrDefaultAsync<Role, RoleCacheItem>(u => u.Id == "3c9310b7-a6d4-42cb-9cdd-adf5718a98b9"));
        }
        public async Task<string> Index7()
        {
            var dd= await _roleListCache.GetAllAsync();
            return "ok";
        }
    }
}

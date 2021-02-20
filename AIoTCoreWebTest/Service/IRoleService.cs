using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AIoT.Core.Dto;
using AIoT.Core.Repository;
using AIoT.Core.Service;
using AIoTCoreWebTest.Service;

namespace AIoT.Module.SysManager.Application
{
    /// <inheritdoc />
    public class RoleService : ServiceBase, IRoleService
    {
        private readonly IRepository<Role,string> _roleRepository;

        public RoleService(IRepository<Role, string> roleRepository)
        {
            _roleRepository = roleRepository;
        }




        /// <summary>
        /// 根据角色权限获取角色数据
        /// </summary>
        /// <returns></returns>
        public async Task<ListResult<RoleDto>> GetRoleAllAsync()
        {
            
            {
                return await _roleRepository.ToListAsync<Role,RoleDto>(u=>u.IsDeleted==false,null);
            }
            
        }
        public async Task<Result<RoleDto>> GetRoleAsync(string id)
        {
            var entity = await _roleRepository.FirstOrDefaultAsync<Role,RoleDto>(u=>u.Id==id);
            if (entity == null)
            {
                return Result.FromError<RoleDto>("");
            }

            return Result.FromData(entity);
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<Result> AddAsync(RoleDto input)
        {

            Role entity = new Role();
            entity.Id = input.Id;
            entity.Name = input.Name;
            entity.Code = input.Code;
            entity.Description = input.Description;

            await _roleRepository.InsertAsync(entity);


            return Result.Ok();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<Result> UpdateAsync(RoleDto input)
        {
            
            var entity = await _roleRepository.GetAsync(input.Id);
            if (entity == null)
            {
                return Result.FromError("");
            }

            entity.Name = input.Name;
            entity.Description = input.Description;

            await _roleRepository.UpdateAsync(entity);
            return Result.Ok();
        }

        public async Task<Result> DeleteAsync(string id)
        {

             await _roleRepository.DeleteAsync(id);
           
            return Result.Ok();
        }

    }
}

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
    public interface IRoleService : IServiceBase
    {

        /// <summary>
        /// 根据角色权限获取角色数据
        /// </summary>
        /// <returns></returns>
        Task<ListResult<RoleDto>> GetRoleAllAsync()
        ;
         Task<Result<RoleDto>> GetRoleAsync(string id)
        ;

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
         Task<Result> AddAsync(RoleDto input)
        ;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
         Task<Result> UpdateAsync(RoleDto input)
       ;

        Task<Result> DeleteAsync(string id);



    }
}

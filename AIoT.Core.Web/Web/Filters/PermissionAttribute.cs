using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AIoT.Core.Authorization;
using AIoT.Core.Dto;
using AIoT.Core.Runtime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AIoT.Core.Web
{
    /// <summary>
    /// 功能权限验证
    /// 权限编码：模块名称.业务名称.业务功能
    /// 基本业务功能：Get、Add、Edit、Query、Delete、Export、Import、Print
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class PermissionAttribute : Attribute, IAsyncAuthorizationFilter, IOrderedFilter
    {
        /// <summary>
        /// 权限编码
        /// </summary>
        public IList<string> Permissions { get; }

        /// <inheritdoc />
        public int Order { get; set; }

        /// <summary>
        /// 功能权限验证
        /// </summary>
        /// <param name="permissions">权限编码</param>
        public PermissionAttribute(params string[] permissions)
        {
            Permissions = permissions ?? new string[0];
            Order = 0;
        }

        /// <inheritdoc />
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                // 未登录
                //context.Result = new StatusCodeResult((int)HttpStatusCode.Unauthorized);
                var result = new ErrorResult();
                result.ByCode(ResultCode.Unauthorized, ResultCode.Unauthorized.DisplayName());
                context.Result = new ObjectResult(result);
            }
            else
            if (Permissions.Count > 0)
            {
                var serviceProvider = context.HttpContext.RequestServices;
                var permissionCheck = serviceProvider.GetRequiredService<IPermissionChecker>();

                var isGranted = false;
                foreach (var item in Permissions)
                {
                    if (item != null && await permissionCheck.IsGrantedAsync(item))
                    {
                        isGranted = true;
                        break;
                    }
                }

                if (!isGranted)
                {
                    // 权限不足
                    var user = context.HttpContext.RequestServices.GetRequiredService<ICurrentUser>();
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<PermissionAttribute>>();
                    logger.LogWarning($"用户权限不足:{user.Name} {user.UserName} {user.UserId}\r\n" +
                                      $"所需要权限:{string.Join(",", Permissions)}");
                    var result = new ErrorResult();
                    result.ByCode(ResultCode.Forbidden, ResultCode.Forbidden.DisplayName());
                    context.Result = new ObjectResult(result);
                    //context.Result = new StatusCodeResult((int)HttpStatusCode.Forbidden);
                }
            }
        }
    }

    /// <summary>
    /// 功能权限验证
    /// 权限编码：模块名称.业务名称.业务功能
    /// 基本业务功能：Get、Add、Edit、Query、Delete、Export、Import、Print
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class PermissionPathAttribute : Attribute, IAsyncAuthorizationFilter, IOrderedFilter
    {
      

        /// <inheritdoc />
        public int Order { get; set; }

        /// <summary>
        /// 功能权限验证
        /// </summary>
        /// <param name="permissions">权限编码</param>
        public PermissionPathAttribute()
        {
            Order = 0;
        }

        /// <inheritdoc />
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                // 未登录
                //context.Result = new StatusCodeResult((int)HttpStatusCode.Unauthorized);
                var result = new ErrorResult();
                result.ByCode(ResultCode.Unauthorized, ResultCode.Unauthorized.DisplayName());
                context.Result = new ObjectResult(result);
            }
            else
            {
                var path = context.HttpContext.Request.Path.Value;
                var serviceProvider = context.HttpContext.RequestServices;
                var permissionCheck = serviceProvider.GetRequiredService<IPermissionChecker>();

                var isGranted = await permissionCheck.IsGrantedPathAsync(path.ToLower());


                if (!isGranted)
                {
                    // 权限不足
                    var user = context.HttpContext.RequestServices.GetRequiredService<ICurrentUser>();
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<PermissionAttribute>>();
                    logger.LogWarning($"用户权限不足:{user.Name} {user.UserName} {user.UserId}\r\n" +
                                      $"所需要权限:{path.ToLower()}");
                    var result = new ErrorResult();
                    result.ByCode(ResultCode.Forbidden, ResultCode.Forbidden.DisplayName());
                    context.Result = new ObjectResult(result);
                    //context.Result = new StatusCodeResult((int)HttpStatusCode.Forbidden);
                }
            }
        }
    }
}

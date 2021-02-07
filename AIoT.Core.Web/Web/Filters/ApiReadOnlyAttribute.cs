using System;
using System.Collections.Generic;
using AIoT.Core.Data;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Data;

namespace AIoT.Core.Web
{
    /// <summary>
    /// 使用读库
    /// </summary>
    public class ApiReadOnlyAttribute : ActionFilterAttribute
    {


        /// <summary>
        /// 数据权限控制
        /// </summary>
        public ApiReadOnlyAttribute()
        {

            Order = -99999998;
        }

        private List<IDisposable> _dateFilterDisposables;

        /// <inheritdoc />
        public override void OnActionExecuting(ActionExecutingContext context)
        {


            var service = context.HttpContext.RequestServices;
            var dataFilter = service.GetRequiredService<IDataFilter>();
            _dateFilterDisposables = new List<IDisposable>();
            _dateFilterDisposables.Add(dataFilter.Enable<IReadonly>());
            ;
        }

        /// <inheritdoc />
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            foreach (var item in _dateFilterDisposables)
            {
                item.Dispose();
            }
        }
    }
}

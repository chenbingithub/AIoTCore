using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Options;
using Volo.Abp;

namespace AIoT.Core.DataFilter
{
    /// <summary>
    /// 上下文数据状态
    /// </summary>
    public interface IDataState
    {
        /// <summary>
        /// 获取当前数据值
        /// </summary>
        T GetData<T>(string key, T defaultValue = default);

        /// <summary>
        /// 设置上下文数据值
        /// </summary>
        IDisposable UseData<T>(string key, T value);
    }
    /// <summary>
    /// 数据权限条件数据
    /// </summary>
    public static class DataStateExtenstion
    {
        /// <summary>
        /// 是否已启用
        /// </summary>
        public static bool IsEnabled(this IDataState dataState, string key)
        {
            return dataState.GetData(key, false);
        }

        /// <summary>
        /// 启用
        /// </summary>
        public static IDisposable Enable(this IDataState dataState, string key)
        {
            return dataState.UseData(key, true);
        }

        /// <summary>
        /// 禁用
        /// </summary>
        public static IDisposable Disable(this IDataState dataState, string key)
        {
            return dataState.UseData(key, false);
        }
    }
   
}

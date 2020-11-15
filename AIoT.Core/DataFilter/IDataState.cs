using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Options;
using AIoT.Core.EntityFrameworkCore;
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
    public class DataState : IDataState
    {
        private readonly ConcurrentDictionary<string, AsyncLocalDataStore> _datas;
        private readonly DataStateOptions _options;

        /// <inheritdoc />
        public DataState(IOptions<DataStateOptions> options)
        {
            _options = options.Value;
            _datas = new ConcurrentDictionary<string, AsyncLocalDataStore>();
        }

        /// <inheritdoc />
        public T GetData<T>(string key, T defaultValue = default)
        {
            return GetStore(key).GetValue(defaultValue);
        }

        /// <inheritdoc />
        public IDisposable UseData<T>(string key, T value)
        {
            return GetStore(key).UseValue(value);
        }

        private AsyncLocalDataStore GetStore(string key)
        {
            return _datas.GetOrAdd(key, p => new AsyncLocalDataStore(key, _options));
        }
    }

    public class NullDataState : IDataState, IDisposable
    {
        public static readonly IDataState Instanse = new NullDataState();

        /// <inheritdoc />
        public T GetData<T>(string key, T defaultValue = default)
        {
            return defaultValue;
        }

        /// <inheritdoc />
        public IDisposable UseData<T>(string key, T value)
        {
            return this;
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }
    internal class AsyncLocalDataStore
    {
        private readonly AsyncLocal<object> _state = new AsyncLocal<object>();
        private readonly object _defaultValue;

        public AsyncLocalDataStore(string key, DataStateOptions options)
        {
            options.DefaultStates.TryGetValue(key, out _defaultValue);
        }

        public object Value
        {
            get => _state.Value ?? _defaultValue;
            set => _state.Value = value;
        }


        public T GetValue<T>(T defaultValue = default)
        {
            var objVal = Value;
            return objVal is T val ? val : defaultValue;
        }

        public IDisposable UseValue<T>(T val)
        {
            var oldVal = Value;
            if (Equals(oldVal, val)) return NullDisposable.Instance;

            Value = val;

            return new DisposeAction(() => Value = oldVal);
        }
    }
    /// <summary>
    /// 数据状态配置
    /// </summary>
    public class DataStateOptions
    {
        /// <inheritdoc />
        public DataStateOptions()
        {
            DefaultStates = new Dictionary<string, object>();
        }

        /// <summary>
        /// 数据默认值
        /// </summary>
        public Dictionary<string, object> DefaultStates { get; }
    }
    public sealed class NullDisposable : IDisposable
    {
        public static NullDisposable Instance { get; } = new NullDisposable();

        private NullDisposable()
        {

        }

        public void Dispose()
        {

        }
    }
}

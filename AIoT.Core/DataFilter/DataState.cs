using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Options;
using Volo.Abp;

namespace AIoT.Core.DataFilter
{
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

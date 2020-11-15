using System;
using System.Threading;
using AIoT.Core.DataFilter;

namespace AIoT.Core.EntityFrameworkCore
{
    /// <summary>
    /// 上下文环境中传递 <see cref="CancellationToken"/>
    /// </summary>
    public interface ICancellationTokenAccessor
    {
        /// <summary>
        /// 获取 <see cref="CancellationToken"/>
        /// </summary>
        CancellationToken Token { get; }

        /// <summary>
        /// 使用指定的 <see cref="CancellationToken"/>
        /// </summary>
        IDisposable UseCancellation(CancellationToken cancellation);
    }
    /// <inheritdoc cref="ICancellationTokenAccessor"/>
    public class DataStateCancellationTokenAccessor : ICancellationTokenAccessor
    {
        public const string CancellationTokenAccessorKey = "CancellationTokenAccessor";
        private readonly IDataState _dataState;

        /// <inheritdoc cref="ICancellationTokenAccessor"/>
        public DataStateCancellationTokenAccessor(IDataState dataState)
        {
            _dataState = dataState;
        }

        /// <inheritdoc />
        public CancellationToken Token => _dataState.GetData(
            CancellationTokenAccessorKey, CancellationToken.None);

        /// <inheritdoc />
        public IDisposable UseCancellation(CancellationToken cancellation)
        {
            return _dataState.UseData(CancellationTokenAccessorKey, cancellation);
        }
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Volo.Abp.DependencyInjection;

namespace AIoT.RedisCache.Cache.Internal
{
    /// <summary>
    /// 缓存存储类
    /// </summary>
    public class CacheStorage : ICacheStorage, IHostedService, IDisposable
    {
        private readonly CacheOptions _options;
        private IMemoryCache _memoryCache;
        private ISubscriber _subscriber;
        private ChannelMessageQueue _channelMessageQueue;
        private bool _isDisposed;

        public CacheStorage(IOptions<CacheOptions> options, IMemoryCache memoryCache)
        {
            _options = options.Value;
            _memoryCache = memoryCache;

            var con = _options.Connection ?? throw new InvalidOperationException($"缺少配置：{nameof(CacheOptions)}.{nameof(CacheOptions.Connection)} 不能为空");
            _subscriber = con.GetSubscriber();
        }

        /// <inheritdoc />
        public IMemoryCache GetMemoryCache()
        {
            CheckDisposed();
            return _memoryCache;
        }

        /// <inheritdoc />
        public IDatabase GetRedisCache()
        {
            CheckDisposed();
            return _options.Connection.GetDatabase(_options.Db);
        }

        /// <inheritdoc />
        public void PublishCacheExpired(string key)
        {
            _subscriber.Publish($"{_options.Prefix}:{CacheBase.CacheExpiredChannel}", key);
        }

        /// <inheritdoc />
        public async Task PublishCacheExpiredAsync(string key)
        {
            await _subscriber.PublishAsync($"{_options.Prefix}:{CacheBase.CacheExpiredChannel}", key);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_isDisposed) return;

            _channelMessageQueue?.Unsubscribe();
            _channelMessageQueue = null;
            _subscriber = null;
            _memoryCache = default;

            _isDisposed = true;
        }

        /// <summary>
        /// 订阅Redis缓存过期消息
        /// </summary>
        private void OnCacheExpired(ChannelMessage channelMessage)
        {
            if (_isDisposed) return;

            string key = channelMessage.Message;
            _memoryCache.Remove(key);
        }

        /// <summary>
        /// 检查是否已释放
        /// </summary>
        private void CheckDisposed()
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(CacheStorage));
        }

        /// <inheritdoc />
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _channelMessageQueue = await _subscriber.SubscribeAsync($"{_options.Prefix}:{CacheBase.CacheExpiredChannel}");
            _channelMessageQueue.OnMessage(OnCacheExpired);
        }

        /// <inheritdoc />
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_isDisposed) return;
            await _channelMessageQueue.UnsubscribeAsync();
        }
    }
}

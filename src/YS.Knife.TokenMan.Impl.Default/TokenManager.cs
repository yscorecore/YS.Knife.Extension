
using System;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using YS.Knife.Lock;

namespace YS.Knife.TokenMan.Impl.Default
{
    [Service]
    [AutoConstructor]
    public partial class TokenManager : ITokenManager
    {
        private readonly IDistributedCache cache;
        private readonly ILockService lockService;
        private readonly ILogger<TokenManager> logger;
        private const string CacheKeyPrefix = "TokenMan:";

        private const int MaxLockSeconds = 120;
        private static JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {

        };
        public async Task<T> GetToken<T>(string name, Func<CancellationToken, Task<T>> tokenFun, Func<T, DateTimeOffset> expiredTimeFunc, int bufferSeconds = 300, int preloadSeconds = 60, CancellationToken cancellationToken = default)
            where T : class
        {
            var key = $"{CacheKeyPrefix}{name}";
            var value = await cache.GetObjectAsync<T>(key, JsonOptions);
            if (value == null)
            {
                return await GetNewTokenWithLock(key, tokenFun, expiredTimeFunc, bufferSeconds, cancellationToken);
            }
            else
            {
                var expiredTime = expiredTimeFunc(value).AddSeconds(-bufferSeconds);
                if ((expiredTime - DateTimeOffset.Now).TotalSeconds < preloadSeconds)
                {
                    //需要刷新
                    return await TryRefreshTokenWithLock(key, tokenFun, expiredTimeFunc, bufferSeconds, value, cancellationToken) ?? value;
                }
                else
                {
                    return value;
                }
            }
        }
        private async Task<T> GetNewTokenWithLock<T>(string key, Func<CancellationToken, Task<T>> tokenFun, Func<T, DateTimeOffset> expiredTimeFunc, int bufferSeconds, CancellationToken cancellationToken)
                where T : class
        {
            var (lockKey, lockAcquired) = ($"lock:{key}", false);
            try
            {
                lockAcquired = await lockService.Lock(lockKey, TimeSpan.FromSeconds(MaxLockSeconds));
                if (lockAcquired)
                {
                    var cacheVal = await cache.GetObjectAsync<T>(key, JsonOptions);
                    if (cacheVal != null)
                    {
                        return cacheVal;
                    }
                    var newValue = await tokenFun(cancellationToken);

                    var expiredTime = expiredTimeFunc(newValue).AddSeconds(-bufferSeconds);
                    await cache.SetObjectAsync(key, newValue, new DistributedCacheEntryOptions { AbsoluteExpiration = expiredTime }, JsonOptions);
                    logger.LogInformation("Token updated for key '{key}'", key);
                    return newValue;
                }
                else
                {
                    await lockService.WaitFor(lockKey);
                    var cachedValue = await cache.GetObjectAsync<T>(key, JsonOptions);
                    return cachedValue ?? throw new InvalidOperationException($"Token for {key} not found after waiting for lock");
                }
            }
            finally
            {
                if (lockAcquired)
                {
                    await lockService.UnLock(lockKey);
                }
            }
        }
        private async Task<T> TryRefreshTokenWithLock<T>(string key, Func<CancellationToken, Task<T>> tokenFun, Func<T, DateTimeOffset> expiredTimeFunc, int bufferSeconds, T defaultValue, CancellationToken cancellationToken)
                where T : class
        {
            var (lockKey, lockAcquired) = ($"lock:{key}", false);
            try
            {
                lockAcquired = await lockService.Lock(lockKey, TimeSpan.FromSeconds(MaxLockSeconds));
                if (lockAcquired)
                {
                    var newValue = await tokenFun(cancellationToken);
                    var expiredTime = expiredTimeFunc(newValue).AddSeconds(-bufferSeconds);
                    await cache.SetObjectAsync(key, newValue, new DistributedCacheEntryOptions { AbsoluteExpiration = expiredTime }, JsonOptions);
                    logger.LogInformation("Token updated for key '{key}'", key);
                    return newValue;
                }
                else
                {
                    return defaultValue;
                }
            }
            finally
            {
                if (lockAcquired)
                {
                    await lockService.UnLock(lockKey);
                }
            }
        }
    }
}

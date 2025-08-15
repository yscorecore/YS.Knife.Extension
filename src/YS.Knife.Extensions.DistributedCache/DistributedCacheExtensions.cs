using System.Text.Json;

namespace Microsoft.Extensions.Caching.Distributed
{
    public static class DistributedCacheExtensions
    {
        public static async Task<T> GetObjectAsync<T>(this IDistributedCache cache, string key, JsonSerializerOptions options = default)
        {
            var content = await cache.GetStringAsync(key);
            return content.AsJsonObject<T>(options);
        }
        public static async Task<T> GetOrAddObjectAsync<T>(this IDistributedCache cache, string key, Func<Task<T>> valueFactory, TimeSpan maxCacheTimeSpan, bool cacheDefaultValue = false, JsonSerializerOptions options = default)
        {
            var content = await cache.GetStringAsync(key);
            if (string.IsNullOrEmpty(content))
            {
                var val = await valueFactory();
                if (val != null || cacheDefaultValue)
                {
                    var body = val.ToJsonText(options);
                    await cache.SetStringAsync(key, body, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = maxCacheTimeSpan });
                }
                return val;
            }
            else
            {
                return content.AsJsonObject<T>(options);
            }

        }

        public static Task<T> GetOrAddObjectAsync<T>(this IDistributedCache cache, string key, T value, TimeSpan maxCacheTimeSpan, bool cacheDefaultValue = false, JsonSerializerOptions options = default)
        {
            return cache.GetOrAddObjectAsync(key, () => Task.FromResult(value), maxCacheTimeSpan, cacheDefaultValue, options);
        }
        public static async Task SetObjectAsync<T>(this IDistributedCache cache, string key, T data, TimeSpan maxCacheTimeSpan, JsonSerializerOptions options = default)
        {
            await cache.SetStringAsync(key, data.ToJsonText(options), new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = maxCacheTimeSpan });
        }
        public static async Task SetObjectAsync<T>(this IDistributedCache cache, string key, T data, DistributedCacheEntryOptions entryOptions, JsonSerializerOptions options = default)
        {
            await cache.SetStringAsync(key, data.ToJsonText(options), entryOptions);
        }
        public static async Task SetObjectAsync<T>(this IDistributedCache cache, string key, T data, JsonSerializerOptions options = default)
        {
            await cache.SetStringAsync(key, data.ToJsonText(options), new DistributedCacheEntryOptions());
        }
        public static async Task SetStringAsync(this IDistributedCache cache, string key, string content, TimeSpan maxCacheTimeSpan)
        {
            await cache.SetStringAsync(key, content, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = maxCacheTimeSpan });
        }
    }
}

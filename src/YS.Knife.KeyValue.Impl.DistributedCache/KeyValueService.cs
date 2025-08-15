using Microsoft.Extensions.Caching.Distributed;

namespace YS.Knife.KeyValue.Impl.DistributedCache
{
    [AutoConstructor]
    [Service]
    public partial class KeyValueService : IKeyValueService
    {
        private readonly IDistributedCache cache;
        private readonly KeyValueOptions keyValueOptions;
        public Task Delete(string key, CancellationToken cancellationToken = default)
        {
            return cache.RemoveAsync(key, cancellationToken);
        }
        public Task<T> GetValue<T>(string key, CancellationToken cancellationToken = default)
        {
            return cache.GetObjectAsync<T>(key, keyValueOptions.JsonSerializerOptions);
        }
        public Task SetValue(string key, object value, bool _, CancellationToken cancellationToken = default)
        {
            return cache.SetObjectAsync(key, value, keyValueOptions.JsonSerializerOptions);
        }
    }
}

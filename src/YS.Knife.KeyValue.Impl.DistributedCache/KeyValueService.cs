using Microsoft.Extensions.Caching.Distributed;

namespace YS.Knife.KeyValue.Impl.DistributedCache
{
    [AutoConstructor]
    [Service]
    public partial class KeyValueService : IKeyValueService
    {
        private readonly IDistributedCache cache;
        public Task Delete(string key, CancellationToken cancellationToken = default)
        {
            return cache.RemoveAsync(key, cancellationToken);
        }

        public Task<string> GetValue(string key, CancellationToken cancellationToken = default)
        {
            return cache.GetStringAsync(key, cancellationToken);
        }

        public Task SetValue(string key, string value, CancellationToken cancellationToken = default)
        {
            return cache.SetStringAsync(key, value, cancellationToken);
        }
    }
}


using Microsoft.EntityFrameworkCore;
using YS.Knife.Entity;

namespace YS.Knife.KeyValue.Impl.EFCore
{
    [AutoConstructor]
    [EntityKeyService]
    public partial class KeyValueService<TKey> : IKeyValueService
        where TKey : notnull
    {
        private readonly IEntityStore<KeyValueEntity<TKey>> keyValueEntityStore;
        public async Task Delete(string key, CancellationToken cancellationToken = default)
        {
            var entity = await keyValueEntityStore.Current.Where(p => p.Key == key).FirstOrDefaultAsync(cancellationToken);
            if (entity != null)
            {
                keyValueEntityStore.Delete(entity);
                await keyValueEntityStore.SaveChangesAsync(cancellationToken);
            }
        }

        public Task<string> GetValue(string key, CancellationToken cancellationToken = default)
        {
            return keyValueEntityStore.Current
                .Where(p => p.Key == key)
                .Select(p => p.Value)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task SetValue(string key, string value, CancellationToken cancellationToken = default)
        {
            var entity = await keyValueEntityStore.Current.Where(p => p.Key == key).FirstOrDefaultAsync(cancellationToken);
            if (entity != null)
            {
                entity.Value = value;
            }
            else
            {
                keyValueEntityStore.Add(new KeyValueEntity<TKey>
                {
                    Key = key,
                    Value = value
                });
            }
            await keyValueEntityStore.SaveChangesAsync(cancellationToken);
        }
    }

}

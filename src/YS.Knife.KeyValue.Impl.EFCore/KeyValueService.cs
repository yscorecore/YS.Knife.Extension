using System.Text.Json;
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
        private readonly KeyValueOptions keyValueOptions;
        public async Task Delete(string key, CancellationToken cancellationToken = default)
        {
            var entity = await keyValueEntityStore.Current.Where(p => p.Key == key).FirstOrDefaultAsync(cancellationToken);
            if (entity != null)
            {
                keyValueEntityStore.Delete(entity);
                await keyValueEntityStore.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<T> GetValue<T>(string key, CancellationToken cancellationToken = default)
        {
            var res = await keyValueEntityStore.Current
                .Where(p => p.Key == key)
                .FirstOrDefaultAsync(cancellationToken);
            if (res != null)
            {
                return res.KeepString ? (T)(object)res.Value : res.Value.AsJsonObject<T>(keyValueOptions.JsonSerializerOptions);
            }
            return default;

        }

        public async Task SetValue(string key, object value, bool keepString, CancellationToken cancellationToken = default)
        {
            var entity = await keyValueEntityStore.Current.Where(p => p.Key == key).FirstOrDefaultAsync(cancellationToken);
            if (entity != null)
            {
                var str = default(string);
                entity.KeepString = keepString && IsString(value, out str);
                entity.Value = entity.KeepString ? str : value.ToJsonText(keyValueOptions.JsonSerializerOptions);
            }
            else
            {
                var str = default(string);
                var isString = keepString && IsString(value, out str);
                keyValueEntityStore.Add(new KeyValueEntity<TKey>
                {
                    Key = key,
                    Value = isString ? str : value.ToJsonText(keyValueOptions.JsonSerializerOptions),
                    KeepString = isString
                });
            }
            await keyValueEntityStore.SaveChangesAsync(cancellationToken);
            bool IsString(object value, out string str)
            {
                if (value is string s)
                {
                    str = s;
                    return true;
                }
                if (value is JsonElement { ValueKind: JsonValueKind.String } je)
                {
                    str = je.Deserialize<string>();
                    return true;
                }
                str = null;
                return false;
            }
        }

    }

}

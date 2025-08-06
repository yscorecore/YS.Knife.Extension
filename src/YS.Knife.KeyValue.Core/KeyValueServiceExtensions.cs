using System.Text.Json;

namespace YS.Knife.KeyValue
{
    public static class KeyValueServiceExtensions
    {
        private static async Task<string> GetFullKey(KeyValueGroup group, string key, CancellationToken cancellationToken)
        {
            if (group == null)
            {
                return key;
            }
            else
            {
                var keyPrefix = await group.GetKeyPrefix();
                return $"{keyPrefix}::{key}";
            }
        }
        public static async Task<string> GetValue(this IKeyValueService service, KeyValueGroup group, string key, CancellationToken cancellationToken = default)
        {
            var fullKey = await GetFullKey(group, key, cancellationToken);
            return await service.GetValue(fullKey, cancellationToken);
        }
        public static async Task SetValue(this IKeyValueService service, KeyValueGroup group, string key, string value, CancellationToken cancellationToken = default)
        {
            var fullKey = await GetFullKey(group, key, cancellationToken);
            await service.SetValue(fullKey, value, cancellationToken);
        }
        public static async Task Delete(this IKeyValueService service, KeyValueGroup group, string key, CancellationToken cancellationToken = default)
        {
            var fullKey = await GetFullKey(group, key, cancellationToken);
            await service.Delete(fullKey, cancellationToken);

        }
        public static async Task<T> GetValue<T>(this IKeyValueService service, KeyValueGroup group, string key, T defaultValue = default, JsonSerializerOptions options = default, CancellationToken cancellationToken = default)
        {
            var value = await service.GetValue(group, key, cancellationToken);
            return value.AsJsonObject<T>(options) ?? defaultValue;
        }
        public static async Task SetValue<T>(this IKeyValueService service, KeyValueGroup group, string key, T value, JsonSerializerOptions options = default, CancellationToken cancellationToken = default)
        {
            var jsonValue = value.ToJsonText(options);
            await service.SetValue(group, key, jsonValue, cancellationToken);
        }
    }
}

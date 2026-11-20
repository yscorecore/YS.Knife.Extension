using System.Text.Json;

namespace YS.Knife.KeyValue
{
    public static class KeyValueServiceExtensions
    {
        private static string GetFullKey(KeyValueGroup group, string key)
        {
            return group == null ? key : group.BuildUniqueKey(key);
        }
        public static async Task<T> GetValue<T>(this IKeyValueService service, KeyValueGroup group, string key, CancellationToken cancellationToken = default)
        {
            var fullKey = GetFullKey(group, key);
            return await service.GetValue<T>(fullKey, cancellationToken);
        }
        public static async Task SetValue(this IKeyValueService service, KeyValueGroup group, string key, object value, bool keepString, CancellationToken cancellationToken = default)
        {
            var fullKey = GetFullKey(group, key);
            await service.SetValue(fullKey, value, keepString, cancellationToken);
        }
        public static async Task Delete(this IKeyValueService service, KeyValueGroup group, string key, CancellationToken cancellationToken = default)
        {
            var fullKey = GetFullKey(group, key);
            await service.Delete(fullKey, cancellationToken);

        }

        public static async Task SetValue<T>(this IKeyValueService service, KeyValueGroup group, string key, T value, CancellationToken cancellationToken = default)
        {
            await service.SetValue(group, key, value, false, cancellationToken);
        }
    }
}

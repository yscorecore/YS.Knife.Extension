using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;

namespace Microsoft.EntityFrameworkCore
{
    public static class EncryptionKeys
    {
        private static readonly ConcurrentDictionary<string, string> _keys = new();

        public static void Set(string name, string key) => _keys[name] = key;

        public static string Get(string name) => _keys[name];

        public static bool TryGet(string name, out string key) => _keys.TryGetValue(name, out key);

        public static void LoadFromConfiguration(IConfiguration configuration)
        {
            var encryptionKeys = configuration.GetSection(nameof(EncryptionKeys));
            if (encryptionKeys != null)
            {
                foreach (var k in encryptionKeys.GetChildren())
                {
                    Set(k.Key, k.Value);
                }
            }
        }
    }
}

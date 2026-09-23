using System.Collections.Concurrent;

namespace Microsoft.EntityFrameworkCore
{
    public static class EncryptionKeys
    {
        private static readonly ConcurrentDictionary<string, string> _keys = new();

        public static void Set(string name, string key) => _keys[name] = key;

        public static string Get(string name) => _keys[name];

        public static bool TryGet(string name, out string key) => _keys.TryGetValue(name, out key);
    }
}

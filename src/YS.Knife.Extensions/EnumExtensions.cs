using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace System
{
    public static class EnumExtensions
    {
        static readonly ConcurrentDictionary<Type, Dictionary<string, string>> enumNameMappingCache = new ConcurrentDictionary<Type, Dictionary<string, string>>();
        public static string GetDisplayName<T>(this T enumItem)
            where T : struct
        {
            //反射
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("should be a enum type.");
            }
            return GetFieldDescriptionByValue(enumItem);
        }

        private static Dictionary<string, string> GetEnumNameMapping(this Type enumType)
        {
            return enumNameMappingCache.GetOrAdd(enumType, (t) =>
            {
                var fields = t.GetFields(BindingFlags.Static | BindingFlags.Public);
                return fields.ToDictionary(p => p.Name, p => GetDescriptonText(p, p.Name));
            });
        }
        private static string GetFieldDescriptionByValue<T>(T enumItem) where T : struct
        {
            var enumDic = GetEnumNameMapping(typeof(T));
            var names = enumItem.ToString().Split(',', StringSplitOptions.TrimEntries)
                    .Select(p => enumDic.TryGetValue(p, out var r) ? r : p.ToString());
            return string.Join(',', names);
        }
        private static string GetDescriptonText(FieldInfo filed, string defauleName)
        {
            var descAttr = filed?.GetCustomAttribute<DisplayAttribute>(true);
            return descAttr?.Name ?? defauleName;
        }
    }

}

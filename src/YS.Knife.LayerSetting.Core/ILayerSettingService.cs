using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace YS.Knife.Function
{
    public interface ILayerSettingService
    {
        Task<Dictionary<string, object>> GetLayerSetting(string group);

        Task<T> GetLayerSettingObject<T>(string group) where T : class, new();

    }
    public static class LayerSettingServiceExtensions
    {
        public static async Task<T> GetLayerSettingObjectByRoleCodes<T>(this ILayerService layerService, string group, string[] roleCodes) where T : class, new()
        {
            var values = await layerService.GetGroupValues(group, roleCodes);
            var instance = new T();
            SetLayerValues(typeof(T), instance, values);
            return instance;
        }
        public static async Task<object> GetLayerSettingObjectByRoleCodes(this ILayerService layerService, string group, Type type, string[] roleCodes)
        {
            var values = await layerService.GetGroupValues(group, roleCodes);
            var instance = Activator.CreateInstance(type);
            SetLayerValues(type, instance, values);
            return instance;
        }
        public static async Task<Dictionary<string, object>> GetLayerSettingDictionaryByRoleCodes(this ILayerService layerService, string group, string[] roleCodes)
        {
            var values = await layerService.GetGroupValues(group, roleCodes);
            return values.ToDictionary(p => p.Key, p => p.Value.AsJsonObject<object>(ILayerService.JsonOptions));
        }

        private static void SetLayerValues(Type instanceType, object instance, List<GroupValueInfo> layerValues)
        {
            if (!layerValues.Any())
            {
                return;
            }
            var props = instanceType.GetProperties().Where(p => p.CanWrite && !p.IsSpecialName)
                    .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);
            foreach (var kv in layerValues)
            {
                var key = kv.Key;
                var value = kv.Value;
                if (props.TryGetValue(key, out var prop))
                {
                    prop.SetValue(instance, JsonSerializer.Deserialize(value, prop.PropertyType, ILayerService.JsonOptions));
                }
            }
        }



    }
}

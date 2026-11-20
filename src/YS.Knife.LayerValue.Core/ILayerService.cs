using System.Text.Json;

namespace YS.Knife.Function
{
    public interface ILayerService
    {
        public static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        Task<List<LayerValueInfo>> GetLayerValuesByKeys(string group, params string[] keys);
        Task<List<LayerValueInfo>> GetLayerValues(string group, string[] keys, string[] roleCodes);
        Task<List<LayerValueInfo>> GetLayerValuesByRoleCodes(string group, string[] roleCodes);
    }
    public static class LayerServiceExtensions
    {
        public static Task<List<GroupValueInfo>> GetGroupValues(this ILayerService layerService, string group, string[] roleCodes, params string[] keys)
        {
            return layerService.GetLayerValues(group, keys, roleCodes).ToGroupValues();
        }

        public static Task<List<GroupValueInfo>> GetGroupValues(this ILayerService layerService, string group, string[] roleCodes)
        {
            return layerService.GetLayerValuesByRoleCodes(group, roleCodes).ToGroupValues();

        }
        public static List<LayerValueInfo<T>> As<T>(this List<LayerValueInfo> layerValues)
        {
            return layerValues.Select(p => new LayerValueInfo<T> { Key = p.Key, RoleCode = p.RoleCode, Value = p.Value.AsJsonObject<T>(ILayerService.JsonOptions) }).ToList();
        }
        public static async Task<List<LayerValueInfo<T>>> As<T>(this Task<List<LayerValueInfo>> layerValuesTask)
        {
            return (await layerValuesTask).As<T>();
        }
        public static List<GroupValueInfo> ToGroupValues(this IEnumerable<LayerValueInfo> layerValues)
        {
            return layerValues.GroupBy(p => p.Key)
                .Select(p =>
                {
                    var data = p.ToList();
                    return new GroupValueInfo(p.Key, data, data.Select(p => p.Value).First());
                }).ToList();
        }
        public static async Task<List<GroupValueInfo>> ToGroupValues(this Task<List<LayerValueInfo>> layerValuesTask)
        {
            return (await layerValuesTask).ToGroupValues();
        }
        public static async Task<List<GroupValueInfo<T>>> As<T>(this Task<List<GroupValueInfo>> groupValuesTask)
        {
            return (await groupValuesTask).As<T>();
        }
        public static List<GroupValueInfo<T>> As<T>(this List<GroupValueInfo> layerValues)
        {
            return layerValues.Select(p => new GroupValueInfo<T>(p.Key, p.LayerValues, p.Value.AsJsonObject<T>(ILayerService.JsonOptions))).ToList();
        }

    }


}

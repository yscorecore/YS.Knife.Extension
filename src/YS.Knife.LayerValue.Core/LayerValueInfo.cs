using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace YS.Knife.Function
{
    public record LayerValueInfo
    {
        [Key]
        public string Key { get; set; } = null!;
        [Key]
        public string RoleCode { get; set; } = null!;
        public string Value { get; set; } = null!;
    }
    public record LayerValueInfo<T>
    {
        public LayerValueInfo()
        {

        }
        [Key]
        public string Key { get; set; } = null!;
        [Key]
        public string RoleCode { get; set; } = null!;
        public T Value { get; set; } = default!;
    }
    public static class LayerValueInfoExtensions
    {
        public static LayerValueInfo<T> ToLayerValueInfo<T>(this LayerValueInfo layerValueInfo)
        {
            return new LayerValueInfo<T>
            {
                Key = layerValueInfo.Key,
                RoleCode = layerValueInfo.RoleCode,
                Value = layerValueInfo.Value.AsJsonObject<T>(ILayerService.JsonOptions)
            };
        }
        public static IEnumerable<LayerValueInfo<T>> ToLayerValueInfo<T>(this IEnumerable<LayerValueInfo> layerValueInfos)
        {
            return layerValueInfos.Select(p => p.ToLayerValueInfo<T>());
        }
        public static LayerValueInfo<T> ToLayerValueInfo<T>(this LayerValueInfo<object> layerValueInfo)
        {
            return new LayerValueInfo<T>
            {
                Key = layerValueInfo.Key,
                RoleCode = layerValueInfo.RoleCode,
                Value = ((JsonElement)layerValueInfo.Value).GetRawText().AsJsonObject<T>(ILayerService.JsonOptions)
            };
        }
        public static IEnumerable<LayerValueInfo<T>> ToLayerValueInfo<T>(this IEnumerable<LayerValueInfo<object>> layerValueInfos)
        {
            return layerValueInfos.Select(p => p.ToLayerValueInfo<T>());
        }
    }

}

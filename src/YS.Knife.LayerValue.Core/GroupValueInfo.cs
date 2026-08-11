using System.ComponentModel.DataAnnotations;

namespace YS.Knife.Function
{
    public record class GroupValueInfo(string Key, List<LayerValueInfo> LayerValues, string Value)
    {


    }

    public record class GroupValueInfo<T>(string Key, List<LayerValueInfo> LayerValues, T Value)
    {
    }

}

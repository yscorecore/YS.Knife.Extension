using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace YS.Knife.Function
{
    public record FunctionInfo
    {
        [Key]
        [JsonPropertyOrder(0)]
        public string Code { get; set; } = null!;
        [JsonPropertyOrder(2)]
        public string Name { get; set; } = null!;
        [JsonPropertyOrder(4)]
        public string? Description { get; set; } = null!;
        [JsonPropertyOrder(1)]
        public string Type { get; set; } = null!;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        [JsonPropertyOrder(7)]
        public Dictionary<string, object>? Config { get; set; } = null!;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        [JsonPropertyOrder(3)]
        public string? ParentCode { get; set; }
        [JsonPropertyOrder(6)]
        public int Sequence { get; set; }

    }
    public record FunctionAppInfo : FunctionInfo
    {
    }

    public record RoleProviderInfo
    {
        public string[] Providers { get; set; } = null!;
        public LayerValuePolicy Policy { get; set; }
    }
    public enum LayerValuePolicy
    {
        Cover = 0,
        Merge = 1
    }
}

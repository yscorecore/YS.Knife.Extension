using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace YS.Knife.Function
{
    public record FunctionInfo
    {
        [Key]
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; } = null!;
        public string Type { get; set; } = null!;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Dictionary<string, object>? Config { get; set; } = null!;
        public string? ParentCode { get; set; }
        public int Sequence { get; set; }

    }
}

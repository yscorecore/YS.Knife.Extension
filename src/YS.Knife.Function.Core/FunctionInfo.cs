using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace YS.Knife.Function.Core
{
    public record FunctionInfo
    {
        [Key]
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Dictionary<string, object> Config { get; set; }
        public string ParentCode { get; set; }
        public int Sequence { get; set; }

    }
}

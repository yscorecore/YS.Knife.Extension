
using System.Text.Json.Serialization;

namespace YS.Knife.Function
{
    public record FunctionTreeInfo : FunctionInfo
    {
        [JsonPropertyOrder(8)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public List<FunctionTreeInfo>? SubItems { get; set; }
    }
}

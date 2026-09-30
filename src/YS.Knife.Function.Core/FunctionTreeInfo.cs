
using System.Text.Json.Serialization;

namespace YS.Knife.Function
{
    public record FunctionTreeInfo : FunctionInfo
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public List<FunctionTreeInfo>? SubItems { get; set; }
    }
}

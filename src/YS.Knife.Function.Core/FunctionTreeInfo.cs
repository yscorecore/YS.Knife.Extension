
using System.Text.Json.Serialization;

namespace YS.Knife.Function
{
    public record FunctionTreeInfo : FunctionInfo
    {
        public List<FunctionTreeInfo>? SubItems { get; set; }
    }
}

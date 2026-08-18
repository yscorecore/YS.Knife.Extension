
using System.Text.Json.Serialization;

namespace YS.Knife.Function.Core
{
    public record FunctionTreeInfo : FunctionInfo
    {
        public List<FunctionTreeInfo>? SubItems { get; set; }
    }
}


using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace YS.Knife.KeyValue.Impl.EFCore
{
    [Options]
    public class KeyValueOptions
    {
        public JsonSerializerOptions JsonSerializerOptions { get; set; } = new JsonSerializerOptions()
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        };
    }

}

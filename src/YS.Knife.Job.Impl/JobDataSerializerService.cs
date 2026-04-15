using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace YS.Knife.Job.Impl.EFCore
{
    [SingletonPattern]
    internal partial class JobDataSerializerService
    {
        public static JsonSerializerOptions Options = new JsonSerializerOptions()
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
        };

        public string SerializeToString<T>(T item)
        {
            return item.ToJsonText(Options);
        }

        public T DeSerializeFromString<T>(string content)
        {
            return content.AsJsonObject<T>(Options);
        }
    }
}

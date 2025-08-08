using System.Text.Json;
using System.Text.Json.Serialization;

namespace System.Text.Json.Serialization
{
    public class CamelCaseJsonStringEnumConverter : JsonStringEnumConverter
    {
        public CamelCaseJsonStringEnumConverter() : base(JsonNamingPolicy.CamelCase, true)
        {

        }
    }
}

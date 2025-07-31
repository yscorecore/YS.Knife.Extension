using System.Text.Json;
using System.Text.Json.Serialization;

namespace System.Text.Json.Serialization;

public partial class JsonMaskAttribute : JsonConverterAttribute
{
    public override JsonConverter CreateConverter(Type typeToConvert)
    {
        if (typeToConvert != typeof(string))
        {
            throw new Exception("only support for string type.");
        }
        return MaskPropertyConverter.Instance;
    }
    [SingletonPattern]
    partial class MaskPropertyConverter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var converter = options.GetConverter(typeof(string)) as JsonConverter<string>;
            return converter.Read(ref reader, typeToConvert, options);
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            var converter = options.GetConverter(typeof(string)) as JsonConverter<string>;
            converter.Write(writer, "******", options);
        }

    }

}

using System.Text.Json;
using System.Text.Json.Serialization;

namespace YS.Knife.Extensions.Json
{
    public class JsonNameMaskAttribute : JsonConverterAttribute
    {
        public override JsonConverter CreateConverter(Type typeToConvert)
        {
            return new JsonNameMaskConverter();
        }
    }

    public class JsonNameMaskConverter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            if (string.IsNullOrEmpty(value)) return value;
            
            return value.Length switch
            {
                1 => value,
                2 => value[0] + "*",
                3 => $"{value[0]}*{value[2]}",
                _ => $"{value[..2]}{new string('*', value.Length - 3)}{value[^1]}"
            };
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }
    }
}

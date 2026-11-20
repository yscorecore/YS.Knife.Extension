namespace System.Text.Json.Serialization;

public class JsonNameMaskAttribute : JsonConverterAttribute
{
    public JsonNameMaskAttribute(char starChar = '*')
    {
        this.StarChar = starChar;
    }
    public char StarChar { get; }
    public override JsonConverter CreateConverter(Type typeToConvert)
    {
        return new JsonNameMaskConverter() { StarChar = this.StarChar };
    }
    private class JsonNameMaskConverter : JsonConverter<string>
    {

        public char StarChar { get; set; } = '*';
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.GetString();

        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {

            value = value ?? string.Empty;
            var maskValue = value.Length switch
            {
                <= 1 => value,
                2 => $"{value[0]}{StarChar}",
                3 => $"{value[0]}{StarChar}{value[2]}",
                _ => $"{value[..2]}{new string(StarChar, value.Length - 3)}{value[^1]}"
            };

            writer.WriteStringValue(maskValue);
        }
    }
}





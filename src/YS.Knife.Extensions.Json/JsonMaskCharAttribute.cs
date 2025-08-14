namespace System.Text.Json.Serialization;

public class JsonMaskCharAttribute : JsonConverterAttribute
{
    public JsonMaskCharAttribute(int startIndex, int length, char starChar = '*')
    {
        this.StarChar = starChar;
        StartIndex = startIndex;
        Length = length;
    }
    public char StarChar { get; set; }
    public int StartIndex { get; }
    public int Length { get; }
    public override JsonConverter CreateConverter(Type typeToConvert)
    {
        if (typeToConvert != typeof(string))
        {
            throw new Exception("JsonMaskAttribute only support for string type.");
        }
        return new JsonMaskCharConverter() { StartIndex = this.StartIndex, Length = this.Length, StarChar = this.StarChar };
    }

    private class JsonMaskCharConverter : JsonConverter<string>
    {

        public char StarChar { get; set; }
        public int StartIndex { get; set; }
        public int Length { get; set; }
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.GetString();

        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            var startIndex = StartIndex >= 0 ? StartIndex : value.Length + StartIndex;
            var length = Math.Max(0, Length);
            if (startIndex < 0)
            {
                length = Math.Max(0, length + startIndex);
                startIndex = 0;
            }

            if (startIndex + length >= value.Length)
            {
                var newValue = value.Substring(0, startIndex) + new string(StarChar, length);
                writer.WriteStringValue(newValue);
            }
            else
            {
                var newValue = value.Substring(0, startIndex) + new string(StarChar, length) + value.Substring(startIndex + length);
                writer.WriteStringValue(newValue);
            }

        }
    }

}



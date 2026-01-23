using System.Globalization;

namespace System.Text.Json.Serialization
{
    public class JsonDateTimeConverter : JsonConverter<DateTime>
    {
        private readonly string _format;

        public JsonDateTimeConverter(string format)
        {
            _format = format;
        }

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var dateString = reader.GetString();
                if (DateTime.TryParseExact(dateString, _format, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out DateTime result))
                {
                    return result;
                }
            }
            throw new JsonException($"Invalid date format. Expected: {_format}");
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(_format));
        }
    }
    public class JsonDateTimeOffsetConverter : JsonConverter<DateTimeOffset>
    {
        private readonly string _format;

        public JsonDateTimeOffsetConverter(string format)
        {
            _format = format;
        }

        public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var dateString = reader.GetString();
                if (DateTimeOffset.TryParseExact(dateString, _format, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out DateTimeOffset result))
                {
                    return result;
                }
            }
            throw new JsonException($"Invalid date format. Expected: {_format}");
        }

        public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(_format));
        }
    }
    [AttributeUsage(AttributeTargets.Property)]
    public class JsonDateTimeFormatAttribute : JsonConverterAttribute
    {
        public string Format { get; }

        public JsonDateTimeFormatAttribute(string format)
        {
            Format = format;
        }

        public override JsonConverter CreateConverter(Type typeToConvert)
        {
            if (typeToConvert == typeof(DateTimeOffset))
            {
                return new JsonDateTimeOffsetConverter(Format);
            }
            else
            {
                return new JsonDateTimeConverter(Format);
            }

        }
    }
}

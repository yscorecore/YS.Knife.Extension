using System.Globalization;

namespace System.Text.Json.Serialization
{
    public class JsonDateTimeConverter : JsonConverter<DateTime>
    {
        private readonly string[] _formats;

        public JsonDateTimeConverter(params string[] formats)
        {
            if (formats == null || formats.Length == 0)
            {
                throw new ArgumentException("At least one format must be provided.", nameof(formats));
            }
            _formats = formats;
        }

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var dateString = reader.GetString();
                foreach (var format in _formats)
                {
                    if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out DateTime result))
                    {
                        return result;
                    }
                }
            }
            throw new JsonException($"Invalid date format. Expected one of: {string.Join(", ", _formats)}");
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(_formats[0]));
        }
    }

    public class JsonDateTimeOffsetConverter : JsonConverter<DateTimeOffset>
    {
        private readonly string[] _formats;

        public JsonDateTimeOffsetConverter(params string[] formats)
        {
            if (formats == null || formats.Length == 0)
            {
                throw new ArgumentException("At least one format must be provided.", nameof(formats));
            }
            _formats = formats;
        }

        public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var dateString = reader.GetString();
                foreach (var format in _formats)
                {
                    if (DateTimeOffset.TryParseExact(dateString, format, CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out DateTimeOffset result))
                    {
                        return result;
                    }
                }
            }
            throw new JsonException($"Invalid date format. Expected one of: {string.Join(", ", _formats)}");
        }

        public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(_formats[0]));
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class JsonDateTimeFormatAttribute : JsonConverterAttribute
    {
        public string[] Formats { get; }

        /// <summary>
        /// 优先序列化格式（Formats 数组第一个元素）。单格式场景下可直接使用此属性。
        /// </summary>
        public string Format => Formats[0];

        public JsonDateTimeFormatAttribute(params string[] formats)
        {
            if (formats == null || formats.Length == 0)
            {
                throw new ArgumentException("At least one format must be provided.", nameof(formats));
            }
            Formats = formats;
        }

        public override JsonConverter CreateConverter(Type typeToConvert)
        {
            if (typeToConvert == typeof(DateTimeOffset))
            {
                return new JsonDateTimeOffsetConverter(Formats);
            }
            else
            {
                return new JsonDateTimeConverter(Formats);
            }
        }
    }
}

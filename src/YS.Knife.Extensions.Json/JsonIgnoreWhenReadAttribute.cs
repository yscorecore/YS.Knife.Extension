using System.Collections.Concurrent;

namespace System.Text.Json.Serialization
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public partial class JsonIgnoreWhenReadAttribute : JsonConverterAttribute
    {
        private static readonly ConcurrentDictionary<Type, JsonConverter> cache = new();
        public override JsonConverter CreateConverter(Type typeToConvert)
        {
            return cache.GetOrAdd(typeToConvert, (t) =>
            {
                var convertType = typeof(JsonIgnoreWhenReadConverter<>).MakeGenericType(t);
                return Activator.CreateInstance(convertType) as JsonConverter;
            });
        }

        partial class JsonIgnoreWhenReadConverter<T> : JsonConverter<T>
        {
            public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                JsonSerializer.Deserialize<T>(ref reader, options);
                return default;
            }

            public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            {
                JsonSerializer.Serialize(writer, value, options);
            }
        }

    }
}


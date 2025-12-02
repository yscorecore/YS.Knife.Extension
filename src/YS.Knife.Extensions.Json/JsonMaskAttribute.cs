using System.Text.RegularExpressions;

namespace System.Text.Json.Serialization;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public partial class JsonMaskAttribute : JsonConverterAttribute
{
    public JsonMaskAttribute()
    {

    }
    public JsonMaskAttribute(string pattern, string replacement)
    {
        this.Pattern = pattern;
        this.Replacement = replacement;
    }
    public string Pattern { get; set; }
    public string Replacement { get; set; } = "******";
    public override JsonConverter CreateConverter(Type typeToConvert)
    {
        if (typeToConvert != typeof(string))
        {
            throw new Exception("JsonMaskAttribute only support for string type.");
        }
        return new MaskPropertyConverter(Pattern, Replacement);
    }

    partial class MaskPropertyConverter : JsonConverter<string>
    {
        public MaskPropertyConverter(string pattern, string replacement)
        {
            this.Pattern = pattern;
            this.Replacement = replacement;
        }
        public string Pattern { get; }
        public string Replacement { get; set; }
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var converter = options.GetConverter(typeof(string)) as JsonConverter<string>;
            return converter.Read(ref reader, typeToConvert, options);
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            var converter = options.GetConverter(typeof(string)) as JsonConverter<string>;
            if (string.IsNullOrEmpty(Pattern))
            {
                converter.Write(writer, Replacement, options);
            }
            else
            {
                var text = Regex.Replace(value ?? string.Empty, Pattern, Replacement);
                converter.Write(writer, text, options);
            }
        }
    }

}





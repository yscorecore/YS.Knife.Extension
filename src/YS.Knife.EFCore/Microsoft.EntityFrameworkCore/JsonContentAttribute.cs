using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Microsoft.EntityFrameworkCore
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class JsonContentAttribute : Attribute, IModelPropertyAttribute
    {
        public JsonContentAttribute() : base()
        {
        }

        /// <summary>
        /// JSON 序列化时使用的属性名称命名规则，默认保持属性原名称。
        /// </summary>
        public JsonPropertyNamingPolicy NamingPolicy { get; set; } = JsonPropertyNamingPolicy.Default;

        public void Apply(PropertyBuilder property)
        {
            var propertyInfo = property.Metadata.PropertyInfo;
            var jsonOptions = CreateJsonSerializerOptions();
            var valueConverterType = typeof(JsonContentConvert<>).MakeGenericType(propertyInfo.PropertyType);
            var valueConverter = (ValueConverter)Activator.CreateInstance(valueConverterType, jsonOptions);
            if (IsDictionary(propertyInfo))
            {
                var argumentsType = propertyInfo.PropertyType.GetGenericArguments();
                var valueComparerType = typeof(DicValueComparer<,>).MakeGenericType(argumentsType[0], argumentsType[1]);
                var valueComparer = (ValueComparer)Activator.CreateInstance(valueComparerType, jsonOptions);
                property.HasConversion(valueConverter, valueComparer);
            }
            else if (IsList(propertyInfo))
            {
                var listItemType = propertyInfo.PropertyType.GetGenericArguments().Single();
                var valueComparerType = typeof(ListValueComparer<>).MakeGenericType(listItemType);
                var valueComparer = (ValueComparer)Activator.CreateInstance(valueComparerType, jsonOptions);
                property.HasConversion(valueConverter, valueComparer);
            }
            else
            {
                property.HasConversion(valueConverter);
            }


            bool IsDictionary(PropertyInfo v)
            {
                return v.PropertyType.IsGenericType && v.PropertyType.GetGenericTypeDefinition() == typeof(Dictionary<,>);
            }
            bool IsList(PropertyInfo v)
            {
                return v.PropertyType.IsGenericType && v.PropertyType.GetGenericTypeDefinition() == typeof(List<>);
            }
        }

        private JsonSerializerOptions CreateJsonSerializerOptions()
        {
            return new JsonSerializerOptions()
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicyFactory.Resolve(NamingPolicy),
            };
        }

        private class JsonContentConvert<T> : ValueConverter<T, string>
        {
            public JsonContentConvert(JsonSerializerOptions options)
           : base(
               v => JsonSerializer.Serialize<T>(v, options),
               v => string.IsNullOrEmpty(v) ? default : JsonSerializer.Deserialize<T>(v, options)
               )
            {
            }
        }
        private class ListValueComparer<T> : ValueComparer<List<T>>
        {
            public ListValueComparer(JsonSerializerOptions options) : base(
                (c1, c2) => c1.SequenceEqual(c2),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToJsonText(options).AsJsonObject<List<T>>(options))
            {
            }
        }
        private class DicValueComparer<TKey, TValue> : ValueComparer<Dictionary<TKey, TValue>>
        {
            public DicValueComparer(JsonSerializerOptions options) : base(
               (c1, c2) => c1.OrderBy(t => t.Key).SequenceEqual(c2.OrderBy(t => t.Key)),
               c => c.OrderBy(t => t.Key).Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
               c => c.ToJsonText(options).AsJsonObject<Dictionary<TKey, TValue>>(options))
            {
            }
        }
    }
}

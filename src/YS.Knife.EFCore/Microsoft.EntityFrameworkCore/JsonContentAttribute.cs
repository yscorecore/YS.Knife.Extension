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
        public void Apply(PropertyBuilder property)
        {
            var valueConverterType = typeof(JsonContentConvert<>).MakeGenericType(property.Metadata.PropertyInfo.PropertyType);
            if (IsDictionary(property.Metadata.PropertyInfo))
            {
                var argumentsType = property.Metadata.PropertyInfo.PropertyType.GetGenericArguments();
                var valueComparerType = typeof(DicValueComparer<,>).MakeGenericType(argumentsType[0], argumentsType[1]);
                property.HasConversion(valueConverterType, valueComparerType);
            }
            else if (IsList(property.Metadata.PropertyInfo))
            {
                var listItemType = property.Metadata.PropertyInfo.PropertyType.GetGenericArguments().Single();
                var valueComparerType = typeof(ListValueComparer<>).MakeGenericType(listItemType);
                property.HasConversion(valueConverterType, valueComparerType);
            }
            else
            {
                property.HasConversion(valueConverterType);
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

        private class JsonContentConvert<T> : ValueConverter<T, string>
        {
            static JsonSerializerOptions Options = new JsonSerializerOptions()
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                PropertyNameCaseInsensitive = true,
            };
            public JsonContentConvert()
           : base(
               v => JsonSerializer.Serialize<T>(v, Options),
               v => string.IsNullOrEmpty(v) ? default : JsonSerializer.Deserialize<T>(v, Options)
               )
            {
            }
        }
        private class ListValueComparer<T> : ValueComparer<List<T>>
        {
            public ListValueComparer() : base(
                (c1, c2) => c1.SequenceEqual(c2),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToJsonText(default).AsJsonObject<List<T>>(default))
            {
            }
        }
        private class DicValueComparer<TKey, TValue> : ValueComparer<Dictionary<TKey, TValue>>
        {
            public DicValueComparer() : base(
               (c1, c2) => c1.OrderBy(t => t.Key).SequenceEqual(c2.OrderBy(t => t.Key)),
               c => c.OrderBy(t => t.Key).Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
               c => c.ToJsonText(default).AsJsonObject<Dictionary<TKey, TValue>>(default))
            {
            }
        }
    }
}

using System.Globalization;
using System.Text.Json;

namespace System
{
    public static class StringExtensions
    {
        public static T AsJsonObject<T>(this string val, JsonSerializerOptions options = default)
        {
            if (string.IsNullOrEmpty(val))
            {
                return default;
            }
            return JsonSerializer.Deserialize<T>(val, options);
        }
        public static string ToJsonText<T>(this T val, JsonSerializerOptions options = default)
        {
            return JsonSerializer.Serialize(val, options);
        }
        public static string JoinUrl(this string baseUrl, params string[] segments)
        {
            var all = new string[] { baseUrl }.Concat(segments);
            return string.Join("/", all.Where(p => !string.IsNullOrEmpty(p)).Select(p => p.Trim('/')));
        }
        public static string CombinQueryString(this string requestUrl, string query)
        {
            if (!string.IsNullOrEmpty(query))
            {
                if (requestUrl.Contains('?'))
                {
                    return requestUrl + query;
                }
                else
                {
                    return requestUrl + "?" + query;
                }
            }
            return requestUrl;
        }
        public static string JoinString<T>(this IEnumerable<T> items, string sep)
        {
            return string.Join(sep, items);
        }
        public static string WithStyle(this string text, NameStyle style)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }
            return style switch
            {
                NameStyle.Lower => text.ToLowerInvariant(),
                NameStyle.Upper => text.ToUpperInvariant(),
                NameStyle.TitleCase => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(text),
                _ => text
            };
        }
    }
}

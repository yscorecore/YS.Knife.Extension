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
        public static string JoinString<T>(this IEnumerable<T> items, Func<T, string> func, string sep)
        {
            _ = func ?? throw new ArgumentNullException(nameof(func));
            return string.Join(sep, items.Select(func));
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
        /// <summary>
        /// 将字符串截断到指定最大长度。若原字符串长度小于等于最大长度，则返回原字符串。
        /// </summary>
        /// <param name="input">要处理的字符串（可为 null）</param>
        /// <param name="maxLength">最大允许的字符数（必须 >= 0）</param>
        /// <returns>截断后的字符串或原字符串</returns>
        /// <exception cref="ArgumentException">当 maxLength 为负数时抛出</exception>
        public static string Truncate(this string input, int maxLength)
        {
            if (maxLength < 0)
                throw new ArgumentException("Max length cannot be negative.", nameof(maxLength));

            if (string.IsNullOrEmpty(input) || input.Length <= maxLength)
                return input;

            return input.Substring(0, maxLength);
        }
    }
}

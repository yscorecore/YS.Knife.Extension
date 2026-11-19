using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace System
{
    public static class StringExtensions
    {
        public static JsonElement AsJsonElement(this object val)
        {
            if (val is JsonElement jsonElement)
            {
                return jsonElement;
            }
            else
            {
                throw new Exception($"Cannot convert type {val.GetType().FullName} to JsonElement.");
            }
        }
        public static T AsJsonObject<T>(this object val, JsonSerializerOptions options = default)
        {
            if (val is null || val is DBNull)
            {
                return default;
            }
            else if (val is T t)
            {
                return t;
            }
            else if (val is JsonElement je)
            {
                return je.AsJsonObject<T>(options);
            }
            else if (val is string str)
            {
                return str.AsJsonObject<T>(options);
            }
            else
            {
                throw new Exception($"Cannot convert type {val.GetType().FullName} to {typeof(T).FullName}.");
            }
        }
        public static T AsJsonObject<T>(this JsonElement val, JsonSerializerOptions options = default)
        {
            return JsonSerializer.Deserialize<T>(val.GetRawText(), options);
        }
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
                NameStyle.CamelCase => JsonNamingPolicy.CamelCase.ConvertName(text),
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
        private static readonly Regex Base64Regex = new(@"^[A-Za-z0-9+/]*={0,2}$");
        public static bool IsValidBase64Strict(this string input)
        {
            return !string.IsNullOrEmpty(input) && input.Length % 4 == 0 && Base64Regex.IsMatch(input);
        }
        /// <summary>
        /// 比较输入字符串与通配符模式的是否匹配，将根据每个字符进行比较
        /// </summary>
        /// <param name="input">输入字符串</param>
        /// <param name="pattern">模式，允许使用的通配符：?，*，其中? 代表任意一个字符，* 代表零或多个任意字符</param>
        /// <returns></returns>
        public static bool IsMatchWildcard(this string input, string pattern, StringComparison stringcomparison = StringComparison.InvariantCulture)
        {//http://www.codeproject.com/Articles/1088/Wildcard-string-compare-globbing

            if (input == null) throw new ArgumentNullException(nameof(input));
            if (input == null) throw new ArgumentNullException(nameof(pattern));
            int inputIndex = 0;
            int patternIndex = 0;

            //无通配符 * 时，比较算法（）
            while (inputIndex < input.Length && patternIndex < pattern.Length && (pattern[patternIndex] != '*'))
            {
                //if ((pattern[patternIndex] != '?') && (input[inputIndex] != pattern[patternIndex]))
                if ((pattern[patternIndex] != '?') && !string.Equals(input.Substring(inputIndex, 1), pattern.Substring(patternIndex, 1), stringcomparison))
                {//如果模式字符不是通配符，且输入字符与模式字符不相等，则可判定整个输入字串与模式不匹配
                    return false;
                }
                patternIndex++;
                inputIndex++;
                if (patternIndex == pattern.Length && inputIndex < input.Length)
                {
                    return false;
                }
                if (inputIndex == input.Length && patternIndex < pattern.Length)
                {
                    return false;
                }
                if (patternIndex == pattern.Length && inputIndex == input.Length)
                {
                    return true;
                }
            }
            //有通配符 * 时，比较算法
            int mp = 0;
            int cp = 0;
            while (inputIndex < input.Length)
            {
                if (patternIndex < pattern.Length && pattern[patternIndex] == '*')
                {
                    if (++patternIndex >= pattern.Length)
                    {
                        return true;
                    }
                    mp = patternIndex;
                    cp = inputIndex + 1;
                }
                //else if (patternIndex < pattern.Length && ((pattern[patternIndex] == input[inputIndex]) || (pattern[patternIndex] == '?')))
                else if (patternIndex < pattern.Length && (string.Equals(input.Substring(inputIndex, 1), pattern.Substring(patternIndex, 1), stringcomparison) || (pattern[patternIndex] == '?')))
                {
                    patternIndex++;
                    inputIndex++;
                }
                else
                {
                    patternIndex = mp;
                    inputIndex = cp++;
                }
            }
            //当输入字符为空且模式为*时
            while (patternIndex < pattern.Length && pattern[patternIndex] == '*')
            {
                patternIndex++;
            }
            return patternIndex >= pattern.Length ? true : false;

        }//end mehtod
        /// <summary>
        /// 判断字符串是否匹配多个模式中的任意一个
        /// </summary>
        /// <param name="input"></param>
        /// <param name="patterns"></param>
        /// <param name="stringcomparison"></param>
        /// <returns></returns>
        public static bool IsMatchWildcardAnyOne(this string input, IEnumerable<string> patterns, StringComparison stringcomparison = StringComparison.InvariantCulture)
        {
            if (patterns == null) return false;
            foreach (var pattern in patterns)
            {
                var res = IsMatchWildcard(input, pattern, stringcomparison);
                if (res) return true;
            }
            return false;
        }
    }
}

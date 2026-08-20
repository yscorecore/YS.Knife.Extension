using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace YS.Knife.Operations
{
    public static class MethodInfoExtensions
    {
        private static System.Collections.Concurrent.ConcurrentDictionary<MethodInfo, Operation> operationCaches = new System.Collections.Concurrent.ConcurrentDictionary<MethodInfo, Operation>();
        public static Operation GetOperation(this MethodInfo methodInfo)
        {
            Assembly.GetEntryAssembly()?.GetCustomAttribute<AppAttribute>();
            return operationCaches.GetOrAdd(methodInfo, p =>
            {

                var attr = methodInfo.GetCustomAttribute<OperationAttribute>();
                if (attr == null)
                {
                    return new Operation { Id = methodInfo.Name, Description = methodInfo.Name };
                }
                var args = methodInfo.DeclaringType?.GetCustomAttributes<OperationArgumentAttribute>();
                var argDic = args.ToLookup(p => p.Key).ToDictionary(p => p.Key, p => p.Last().Value as object);
                return new Operation
                {
                    Id = attr.Id,
                    Description = FormatTemplate(attr.Description, argDic),
                };
            });
        }




        private static string FormatTemplate(string template, Dictionary<string, object> args)
        {
            return (template == null || args.Count == 0) ? template :
                TextValuesFormatter.FromText(template).Format(Array.Empty<object>(), args);
        }

        class TextValuesFormatter
        {
            private const string NullValue = "[null]";
            private static readonly char[] FormatDelimiters = { ',', ':' };
            private readonly string _format;
            private readonly List<string> _valueNames = new List<string>();

            public TextValuesFormatter(string format)
            {
                OriginalFormat = format ?? throw new ArgumentNullException(nameof(format));

                var sb = new StringBuilder();
                int scanIndex = 0;
                int endIndex = format.Length;

                while (scanIndex < endIndex)
                {
                    int openBraceIndex = FindBraceIndex(format, '{', scanIndex, endIndex);
                    int closeBraceIndex = FindBraceIndex(format, '}', openBraceIndex, endIndex);

                    if (closeBraceIndex == endIndex)
                    {
                        sb.Append(format, scanIndex, endIndex - scanIndex);
                        scanIndex = endIndex;
                    }
                    else
                    {
                        // Format item syntax : { index[,alignment][ :formatString] }.
                        int formatDelimiterIndex =
                            FindIndexOfAny(format, FormatDelimiters, openBraceIndex, closeBraceIndex);

                        sb.Append(format, scanIndex, openBraceIndex - scanIndex + 1);
                        sb.Append(_valueNames.Count.ToString(CultureInfo.InvariantCulture));
                        _valueNames.Add(format.Substring(openBraceIndex + 1, formatDelimiterIndex - openBraceIndex - 1));
                        sb.Append(format, formatDelimiterIndex, closeBraceIndex - formatDelimiterIndex + 1);

                        scanIndex = closeBraceIndex + 1;
                    }
                }

                _format = sb.ToString();
            }

            public string OriginalFormat { get; private set; }
            public List<string> ValueNames => _valueNames;

            private static int FindBraceIndex(string format, char brace, int startIndex, int endIndex)
            {
                // Example: {{prefix{{{Argument}}}suffix}}.
                int braceIndex = endIndex;
                int scanIndex = startIndex;
                int braceOccurrenceCount = 0;

                while (scanIndex < endIndex)
                {
                    if (braceOccurrenceCount > 0 && format[scanIndex] != brace)
                    {
                        if (braceOccurrenceCount % 2 == 0)
                        {
                            // Even number of '{' or '}' found. Proceed search with next occurrence of '{' or '}'.
                            braceOccurrenceCount = 0;
                            braceIndex = endIndex;
                        }
                        else
                        {
                            // An unescaped '{' or '}' found.
                            break;
                        }
                    }
                    else if (format[scanIndex] == brace)
                    {
                        if (brace == '}')
                        {
                            if (braceOccurrenceCount == 0)
                            {
                                // For '}' pick the first occurrence.
                                braceIndex = scanIndex;
                            }
                        }
                        else
                        {
                            // For '{' pick the last occurrence.
                            braceIndex = scanIndex;
                        }

                        braceOccurrenceCount++;
                    }

                    scanIndex++;
                }

                return braceIndex;
            }

            private static int FindIndexOfAny(string format, char[] chars, int startIndex, int endIndex)
            {
                int findIndex = format.IndexOfAny(chars, startIndex, endIndex - startIndex);
                return findIndex == -1 ? endIndex : findIndex;
            }



            public string Format(object[] args, IDictionary<string, object> kwargs)
            {
                if (args == null || args.Length == 0)
                {
                    return Format(kwargs);
                }

                var dic = kwargs != null ? new Dictionary<string, object>(kwargs) : new Dictionary<string, object>();
                for (int i = 0; i < args.Length; i++)
                {
                    dic[i.ToString()] = args[i];
                }

                return Format(dic);
            }

            private string Format(IDictionary<string, object> kwargs)
            {
                var values = this.ValueNames.Select(p =>
                {
                    if (kwargs != null && kwargs.TryGetValue(p, out var value))
                    {
                        return value;
                    }

                    return null;
                }).ToArray();
                return Format(values);
            }

            private string Format(object[] values)
            {
                object[] formatedValues = new object[values == null ? 0 : values.Length];
                if (values != null)
                {
                    for (int i = 0; i < values.Length; i++)
                    {
                        formatedValues[i] = FormatArgument(values[i]);
                    }
                }

                return string.Format(CultureInfo.InvariantCulture, _format, formatedValues ?? Array.Empty<object>());
            }

            private object FormatArgument(object value)
            {
                if (value == null)
                {
                    return NullValue;
                }

                // since 'string' implements IEnumerable, special case it
                if (value is string)
                {
                    return value;
                }

                // if the value implements IEnumerable, build a comma separated string.
                if (value is IEnumerable enumerable)
                {
                    return string.Join(", ", enumerable.Cast<object>().Select(o => o ?? NullValue));
                }

                return value;
            }

            private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, TextValuesFormatter> _localCache =
                new System.Collections.Concurrent.ConcurrentDictionary<string, TextValuesFormatter>();

            public static TextValuesFormatter FromText(string text)
            {
                return _localCache.GetOrAdd(text, t => new TextValuesFormatter(t));
            }
        }


    }
}


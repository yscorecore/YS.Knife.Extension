using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace YS.Knife.FileStorage
{
    [AutoConstructor]
    [SingletonPattern]
    public partial class TemplatePlaceholder
    {
        private static readonly Regex _regex = new(@"(?<user>\$)?\{\s*(?<name>\w+)\s*(:(?<fmt>.+))?\}");
        public string FillPlaceholder(string placeHolder, IDictionary<string, string> userArgs, IDictionary<string, ISystemArgProvider> systemArgs)
        {
            _ = placeHolder ?? throw new ArgumentNullException(nameof(placeHolder));
            return _regex.Replace(placeHolder, match =>
            {
                var name = match.Groups["name"].Value;
                var fmt = match.Groups["fmt"].Value;
                var isUser = match.Groups["user"].Success;
                if (isUser)
                {
                    if (userArgs.TryGetValue(name, out var value))
                    {
                        return FormatValue(value, fmt);
                    }
                    else
                    {
                        throw new Exception($"User argument '{name}' not found. Support user arguments: [{string.Join(", ", userArgs.Keys)}].");
                    }
                }
                else
                {
                    if (systemArgs.TryGetValue(name, out var sysArg))
                    {
                        var format = string.IsNullOrEmpty(fmt) ? sysArg.DefaultFormatter : fmt;
                        return FormatValue(sysArg.GetValue(), format);

                    }
                    else
                    {
                        throw new Exception($"System argument '{name}' not found. Support system arguments: [{string.Join(", ", systemArgs.Keys)}].");
                    }
                }
            });
        }
        private string FormatValue(object value, string fmt)
        {
            if (string.IsNullOrEmpty(fmt)) return value?.ToString() ?? string.Empty;
            if (value is IFormattable formattable)
            {
                return formattable.ToString(fmt, null);
            }
            return value.ToString();
        }
    }
}

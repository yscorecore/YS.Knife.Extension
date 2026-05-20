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
    [Service(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
    public partial class TemplatePlaceholder : ITemplatePlaceholder
    {
        private static readonly Regex _regex = new(@"(?<user>\$)?\{\s*(?<name>\w+)\s*(:(?<fmt>.+?))?\}");
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
                        throw new Exception($"User argument '{name}' not found.");
                    }
                }
                else
                {
                    if (systemArgs.TryGetValue(name, out var sysArg))
                    {
                        var format = string.IsNullOrEmpty(fmt) ? sysArg.DefaultFormatter : fmt;
                        return FormatValue(sysArg.GetValue(), format);
                    }
                    else if (userArgs.TryGetValue(name, out var userArg))
                    {
                        return FormatValue(userArg, fmt);
                    }
                    else
                    {
                        throw new Exception($"Argument '{name}' not found. Support arguments: [{string.Join(", ", systemArgs.Keys.Concat(userArgs.Keys))}].");
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

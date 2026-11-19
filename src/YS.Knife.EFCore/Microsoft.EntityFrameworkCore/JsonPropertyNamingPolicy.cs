using System.Text;
using System.Text.Json;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    /// JSON 序列化时的属性名称命名规则。
    /// </summary>
    public enum JsonPropertyNamingPolicy
    {
        /// <summary>
        /// 保持属性原名称。
        /// </summary>
        Default = 0,
        /// <summary>
        /// 小驼峰命名，如 <c>userName</c>。
        /// </summary>
        CamelCase,
        /// <summary>
        /// 大驼峰命名，如 <c>UserName</c>。
        /// </summary>
        PascalCase,
        /// <summary>
        /// 小写蛇形命名，如 <c>user_name</c>。
        /// </summary>
        SnakeCaseLower,
        /// <summary>
        /// 大写蛇形命名，如 <c>USER_NAME</c>。
        /// </summary>
        SnakeCaseUpper,
        /// <summary>
        /// 小写烤肉串命名，如 <c>user-name</c>。
        /// </summary>
        KebabCaseLower,
        /// <summary>
        /// 大写烤肉串命名，如 <c>USER-NAME</c>。
        /// </summary>
        KebabCaseUpper,
    }

    internal static class JsonNamingPolicyFactory
    {
        public static JsonNamingPolicy Resolve(JsonPropertyNamingPolicy policy)
        {
            return policy switch
            {
                JsonPropertyNamingPolicy.CamelCase => JsonNamingPolicy.CamelCase,
                JsonPropertyNamingPolicy.PascalCase => PascalCaseNamingPolicy.Instance,
                JsonPropertyNamingPolicy.SnakeCaseLower => SeparatorNamingPolicy.SnakeCaseLowerPolicy,
                JsonPropertyNamingPolicy.SnakeCaseUpper => SeparatorNamingPolicy.SnakeCaseUpperPolicy,
                JsonPropertyNamingPolicy.KebabCaseLower => SeparatorNamingPolicy.KebabCaseLowerPolicy,
                JsonPropertyNamingPolicy.KebabCaseUpper => SeparatorNamingPolicy.KebabCaseUpperPolicy,
                _ => null,
            };
        }

        private sealed class PascalCaseNamingPolicy : JsonNamingPolicy
        {
            public static readonly PascalCaseNamingPolicy Instance = new PascalCaseNamingPolicy();

            public override string ConvertName(string name)
            {
                if (string.IsNullOrEmpty(name) || char.IsUpper(name[0]))
                {
                    return name;
                }
                return char.ToUpperInvariant(name[0]) + name.Substring(1);
            }
        }

        private sealed class SeparatorNamingPolicy : JsonNamingPolicy
        {
            public static readonly SeparatorNamingPolicy SnakeCaseLowerPolicy = new SeparatorNamingPolicy('_', false);
            public static readonly SeparatorNamingPolicy SnakeCaseUpperPolicy = new SeparatorNamingPolicy('_', true);
            public static readonly SeparatorNamingPolicy KebabCaseLowerPolicy = new SeparatorNamingPolicy('-', false);
            public static readonly SeparatorNamingPolicy KebabCaseUpperPolicy = new SeparatorNamingPolicy('-', true);

            private readonly char separator;
            private readonly bool uppercase;

            private SeparatorNamingPolicy(char separator, bool uppercase)
            {
                this.separator = separator;
                this.uppercase = uppercase;
            }

            public override string ConvertName(string name)
            {
                if (string.IsNullOrEmpty(name))
                {
                    return name;
                }

                var builder = new StringBuilder(name.Length + 20);
                for (var i = 0; i < name.Length; i++)
                {
                    var c = name[i];
                    if (char.IsUpper(c))
                    {
                        if (i > 0 && name[i - 1] != separator && !char.IsUpper(name[i - 1]))
                        {
                            builder.Append(separator);
                        }
                        else if (i > 0 && i < name.Length - 1 && char.IsUpper(name[i - 1]) && char.IsLower(name[i + 1]))
                        {
                            builder.Append(separator);
                        }
                        builder.Append(uppercase ? c : char.ToLowerInvariant(c));
                    }
                    else
                    {
                        builder.Append(uppercase ? char.ToUpperInvariant(c) : c);
                    }
                }
                return builder.ToString();
            }
        }
    }
}

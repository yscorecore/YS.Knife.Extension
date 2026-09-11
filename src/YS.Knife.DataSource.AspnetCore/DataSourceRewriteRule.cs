using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;

namespace YS.Knife.DataSource.AspnetCore
{
    /// <summary>
    /// datasource 重写规则:把 <c>{DataSourceEndpointTemplate}</c> 形式的路径重写到真实 endpoint,
    /// 同时按 <see cref="DataSourceInfo.Filter"/> 注入 / 合并 query string 里的 <c>filter</c>。
    ///
    /// 内置的 <see cref="RewriteOptions.AddRewrite(string, string, bool)"/> 是纯正则替换,
    /// 没法读请求的 query string 再做条件分支,所以 Filter 不为空时改用这个自定义规则。
    ///
    /// 合并规则(参数名大小写不敏感,<c>filter</c> / <c>Filter</c> 视为同一个参数):
    ///   1. Filter 为空              -> 不在本规则的职责范围(调用方走 AddRewrite),查询字符串原样保留
    ///   2. Filter 有值,请求无 filter -> 追加 <c>filter=(Filter)</c>,值做 URL 转义
    ///   3. Filter 有值,请求有 filter -> <c>filter=(Filter) and (原 filter 反转义后)</c>,整体再转义
    /// </summary>
    internal sealed class DataSourceRewriteRule : IRule
    {
        /// <summary>query string 里 filter 参数的名字(比较时忽略大小写)。</summary>
        private const string FilterKey = "filter";

        /// <summary>匹配超时,与内置 RewriteRule 一致,防止配置里塞入病态模板导致回溯。</summary>
        private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(1);

        private readonly Regex _sourcePattern;
        private readonly string _targetPath;
        private readonly string _filter;

        public DataSourceRewriteRule(string sourcePath, string targetPath, string filter)
        {
            _sourcePattern = new Regex(
                "^" + Regex.Escape(sourcePath) + "/?$",
                RegexOptions.Compiled | RegexOptions.CultureInvariant,
                RegexTimeout);
            _targetPath = targetPath;
            _filter = filter;
        }

        public void ApplyRule(RewriteContext context)
        {
            var request = context.HttpContext.Request;
            var path = request.Path.Value ?? string.Empty;

            // 与内置 RewriteRule 对齐:它匹配的是 path.ToString().Substring(1),
            // 即先砍掉前导 '/' 再套正则,所以这里也要去掉,否则 "^api/datasource/x$" 永远匹配不上 "/api/datasource/x"
            var patternInput = path.Length > 0 && path[0] == '/' ? path[1..] : path;

            if (!_sourcePattern.IsMatch(patternInput))
            {
                // 不匹配 -> 什么都不做,交给后面的规则 / 中间件
                return;
            }

            // 路径重写:PathString 会自动补前导 '/'
            request.Path = new PathString("/" + _targetPath);

            // query string 重写:在原有参数基础上注入 / 合并 filter
            request.QueryString = MergeFilter(request.QueryString, _filter);

            // ContinueRules(枚举默认值) = 继续执行后面的规则,跟 AddRewrite(..., skipRemainingRules: false) 语义一致
            context.Result = RuleResult.ContinueRules;
        }

        /// <summary>
        /// 按规则 2 / 3 计算并重建 query string。
        /// </summary>
        private static QueryString MergeFilter(QueryString original, string filter)
        {
            var pairs = ParseQuery(original.Value);

            var index = pairs.FindIndex(
                pair => string.Equals(pair.Key, FilterKey, StringComparison.OrdinalIgnoreCase));

            if (index >= 0)
            {
                // 规则 3:请求里已有 filter -> 合并成 "(配置值) and (原值)"
                // 注意 ParseQuery 出来的 value 已经是反转义后的明文,这里直接参与拼接,
                // 最后 BuildQueryString 会整体再转义一次
                var originalFilter = pairs[index].Value ?? string.Empty;
                pairs[index] = new KeyValuePair<string, string?>(
                    pairs[index].Key,
                    $"({filter}) and ({originalFilter})");
            }
            else
            {
                // 规则 2:请求里没有 filter -> 直接追加配置值
                pairs.Add(new KeyValuePair<string, string?>(FilterKey, filter));
            }

            return BuildQueryString(pairs);
        }

        /// <summary>
        /// 解析原始 query string(可带前导 '?')为有序键值对,key / value 均做 URL 反转义。
        /// 保留原始 key 的拼写与顺序;形如 <c>?flag</c> 这种没有 '=' 的段保留为 null 值。
        /// </summary>
        private static List<KeyValuePair<string, string?>> ParseQuery(string? rawQueryString)
        {
            var pairs = new List<KeyValuePair<string, string?>>();
            if (string.IsNullOrEmpty(rawQueryString))
            {
                return pairs;
            }

            var raw = rawQueryString[0] == '?' ? rawQueryString[1..] : rawQueryString;
            foreach (var segment in raw.Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                var separator = segment.IndexOf('=');
                if (separator < 0)
                {
                    pairs.Add(new KeyValuePair<string, string?>(Uri.UnescapeDataString(segment), null));
                }
                else
                {
                    var key = Uri.UnescapeDataString(segment[..separator]);
                    var value = Uri.UnescapeDataString(segment[(separator + 1)..]);
                    pairs.Add(new KeyValuePair<string, string?>(key, value));
                }
            }

            return pairs;
        }

        /// <summary>
        /// 把键值对重新拼成 query string,key / value 都做 URL 转义
        /// (用 <see cref="Uri.EscapeDataString(string)"/>,空格转成 %20 而不是 '+'，
        /// 避免 '+' 在 query 里被当成空格的歧义)。
        /// 注意 <see cref="QueryString"/> 的构造函数不会自动补前导 '?',非空值必须自己带。
        /// </summary>
        private static QueryString BuildQueryString(List<KeyValuePair<string, string?>> pairs)
        {
            if (pairs.Count == 0)
            {
                return QueryString.Empty;
            }

            var builder = new StringBuilder("?");
            foreach (var (key, value) in pairs)
            {
                if (builder.Length > 1)
                {
                    builder.Append('&');
                }

                builder.Append(Uri.EscapeDataString(key));
                if (value is not null)
                {
                    builder.Append('=').Append(Uri.EscapeDataString(value));
                }
            }

            return new QueryString(builder.ToString());
        }
    }
}

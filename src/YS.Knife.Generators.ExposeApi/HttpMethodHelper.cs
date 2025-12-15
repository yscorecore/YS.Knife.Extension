using System;
using System.Collections.Generic;
using System.Linq;

namespace YS.Knife.Generators.ExposeApi
{
    /// <summary>
    /// 提供HTTP方法推断帮助功能的工具类
    /// </summary>
    public static class HttpMethodHelper
    {
        /// <summary>
        /// 根据方法名和返回值类型推断HTTP方法类型
        /// </summary>
        /// <param name="methodName">方法名称</param>
        /// <param name="returnTypeName">返回值类型名称</param>
        /// <returns>推断的HTTP方法类型（如HttpGet、HttpPost等）</returns>
        // HTTP方法匹配规则字典（使用前缀数组简化结构）
        private static readonly Dictionary<string, string[]> HttpMethodRules = new Dictionary<string, string[]>
        {
            {
                "HttpGet", new string[] { "get", "query", "find", "fetch" }
            },
            {
                "HttpPost", new string[] { "create", "add", "post" }
            },
            {
                "HttpPut", new string[] { "update", "modify" }
            },
            {
                "HttpDelete", new string[] { "delete", "remove" }
            },
            {
                "HttpPatch", new string[] { "patch" }
            }
        };

        /// <summary>
        /// 根据方法名和返回值类型推断HTTP方法类型
        /// </summary>
        /// <param name="methodName">方法名称</param>
        /// <param name="returnTypeName">返回值类型名称</param>
        /// <returns>推断的HTTP方法类型（如HttpGet、HttpPost等）</returns>
        public static string InferHttpMethod(string methodName, string returnTypeName)
        {
            if (string.IsNullOrEmpty(methodName))
                return string.IsNullOrEmpty(returnTypeName) || returnTypeName.ToLower() == "void" ? "HttpPost" : "HttpGet"; // 根据返回值类型决定默认方法

            var lowerMethodName = methodName.ToLower();

            // 遍历规则字典查找匹配的HTTP方法
            foreach (var rule in HttpMethodRules)
            {
                if (rule.Value.Any(prefix => lowerMethodName.StartsWith(prefix)))
                {
                    return rule.Key;
                }
            }

            // 根据返回值类型决定默认方法
            return string.IsNullOrEmpty(returnTypeName) || returnTypeName.ToLower() == "void" ? "HttpPost" : "HttpGet";
        }


    }
}

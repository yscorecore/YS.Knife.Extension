using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace YS.Knife.Tts.Impl.Aliyun
{
    public class AliyunSignature
    {
        private static string CalcQueryString(IDictionary<string, string> values)
        {
            var stringBuilder = new System.Text.StringBuilder();
            foreach (var key in values.Keys.OrderBy(k => k, StringComparer.Ordinal))
            {
                if (stringBuilder.Length > 0)
                {
                    stringBuilder.Append('&');
                }
                stringBuilder.Append($"{PercentEncode(key)}={PercentEncode(values[key])}");
            }
            return stringBuilder.ToString();
        }
        public static string CalcSignature(string method, string path, IDictionary<string, string> values, string accessKeySecret)
        {
            var queryString = CalcQueryString(values);
            var stringToSign = $"{method}&{PercentEncode(path)}&{PercentEncode(queryString)}";
            using var hmac = new System.Security.Cryptography.HMACSHA1(System.Text.Encoding.UTF8.GetBytes($"{accessKeySecret}&"));
            var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(stringToSign));
            return PercentEncode(Convert.ToBase64String(hash));
        }
        public static string BuildQueryStringWithSignature(string method, string path, IDictionary<string, string> values, string accessKeySecret)
        {
            var queryString = CalcQueryString(values);
            var stringToSign = $"{method}&{PercentEncode(path)}&{PercentEncode(queryString)}";
            using var hmac = new System.Security.Cryptography.HMACSHA1(System.Text.Encoding.UTF8.GetBytes($"{accessKeySecret}&"));
            var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(stringToSign));
            return $"Signature={PercentEncode(Convert.ToBase64String(hash))}&{queryString}";
        }
        static string PercentEncode(string value)
        {
            if (value != null)
            {
                // 使用 HttpUtility.UrlEncode 进行 URL 编码
                string encodedValue = WebUtility.UrlEncode(value);

                // 进行字符替换
                encodedValue = encodedValue.Replace("+", "%20");
                encodedValue = encodedValue.Replace("*", "%2A");
                encodedValue = encodedValue.Replace("%7E", "~");

                return encodedValue;
            }
            return null;
        }
    }
}

using System.Collections;
using System.Data;
using System.Net.Mime;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using YS.Knife;

namespace System.Net.Http
{
    public static class HttpClientExtensions
    {
        private static JsonSerializerOptions DefaultJsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            PropertyNameCaseInsensitive = true,
        };


        public static async Task<string> SendAsString(this HttpClient client, HttpMethod method, string baseUrl, string path, object header = default, object param = default, object body = default, Encoding encoding = default, JsonSerializerOptions jsonOptions = default, bool checkStatusCode = true)
        {
            var response = await SendAsResponse(client, method, baseUrl, path, header, param, body, encoding, jsonOptions);
            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream, encoding ?? Encoding.UTF8);
            var content = await reader.ReadToEndAsync();
            if (checkStatusCode && !response.IsSuccessStatusCode)
            {
                throw CreateHttpRequestException(response, content, method, baseUrl, path);
            }
            return content;
        }
        public static async Task<T> SendAsObject<T>(this HttpClient client, HttpMethod method, string baseUrl, string path, object header = default, object param = default, object body = default, Encoding encoding = default, JsonSerializerOptions jsonOptions = null, bool checkStatusCode = true, Func<T, string, Exception> businessExceptionFactory = default)
        {
            var content = await client.SendAsString(method, baseUrl, path, header, param, body, encoding, jsonOptions, checkStatusCode);
            var res = content.AsJsonObject<T>(jsonOptions ?? DefaultJsonOptions);
            var businessException = businessExceptionFactory?.Invoke(res, content);
            if (businessException != null)
            {
                throw businessException;
            }
            return res;

        }

        private static HttpRequestException CreateHttpRequestException(
                HttpResponseMessage response,
                string content,
                HttpMethod method,
                string baseUrl,
                string path)
        {
            var requestUri = response.RequestMessage?.RequestUri?.ToString() ?? baseUrl.JoinUrl(path);
            var truncatedContent = TruncateContent(content, 2000); // 限制响应内容长度
            var message = $"""
HTTP request failed.
Request: {method} {requestUri}
Status Code: {(int)response.StatusCode} ({response.StatusCode})
Reason Phrase: {response.ReasonPhrase}
        
Response Headers:
{FormatHeaders(response.Headers)}
{(response.Content?.Headers != null ? FormatHeaders(response.Content.Headers) : "")}
        
Response Content (truncated):
{truncatedContent}
        
Request Timestamp: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC
""";
            // 使用带HttpStatusCode的构造函数
            return new HttpRequestException(message, null, response.StatusCode);
            string TruncateContent(string content, int maxLength)
            {
                if (string.IsNullOrEmpty(content) || content.Length <= maxLength)
                    return content;

                return content[..maxLength] + $"\n... [Content truncated, total {content.Length} characters]";
            }
            static string FormatHeaders(IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers)
            {
                if (headers == null) return string.Empty;

                return string.Join(Environment.NewLine,
                    headers.Select(h => $"  {h.Key}: {string.Join(", ", h.Value)}"));
            }

        }
        public static async Task<HttpResponseMessage> SendAsResponse(this HttpClient client, HttpMethod method, string baseUrl, string path, object header = default, object param = default, object body = default, Encoding encoding = null, JsonSerializerOptions jsonOptions = default)
        {
            var queryString = param != null ?
                                string.Join("&", Foreach(param).Select(p => $"{UrlEncoder.Default.Encode(p.Key)}={UrlEncoder.Default.Encode(p.Value)}"))
                                : string.Empty;
            var request = new HttpRequestMessage(method, baseUrl.JoinUrl(path).CombinQueryString(queryString));
            foreach (var (k, v) in Foreach(header))
            {
                request.Headers.Add(k, v);
            }
            if (body != null)
            {
                if (body is HttpContent httpContent)
                {
                    request.Content = httpContent;
                }
                else
                {
                    request.Content = new StringContent(body.ToJsonText(jsonOptions ?? DefaultJsonOptions), encoding: encoding ?? Encoding.UTF8, mediaType: MediaTypeNames.Application.Json);
                }
            }
            var response = await client.SendAsync(request);
            return response;
        }

        public static Task<string> GetAsString(this HttpClient client, string baseUrl, string path, object header = default, object param = default, object body = default, Encoding encoding = default, JsonSerializerOptions jsonOptions = default, bool checkStatusCode = true)
        {
            return client.SendAsString(HttpMethod.Get, baseUrl, path, header, param, body, encoding, jsonOptions, checkStatusCode);

        }
        public static async Task<T> GetAsObject<T>(this HttpClient client, string baseUrl, string path, object header = default, object param = default, object body = default, Encoding encoding = default, JsonSerializerOptions jsonOptions = null, bool checkStatusCode = true, Func<T, string, Exception> businessExceptionFactory = default)
        {
            var content = await client.GetAsString(baseUrl, path, header, param, body, encoding, jsonOptions, checkStatusCode);
            var res = content.AsJsonObject<T>(jsonOptions ?? DefaultJsonOptions);
            var businessException = businessExceptionFactory?.Invoke(res, content);
            if (businessException != null)
            {
                throw businessException;
            }
            return res;

        }
        public static Task<string> DeleteAsString(this HttpClient client, string baseUrl, string path, object header = default, object param = default, object body = default, Encoding encoding = default, JsonSerializerOptions jsonOptions = default, bool checkStatusCode = true)
        {
            return client.SendAsString(HttpMethod.Delete, baseUrl, path, header, param, body, encoding, jsonOptions, checkStatusCode);

        }
        public static Task<T> DeleteAsObject<T>(this HttpClient client, string baseUrl, string path, object header = default, object param = default, object body = default, Encoding encoding = default, JsonSerializerOptions jsonOptions = null, bool checkStatusCode = true, Func<T, string, Exception> businessExceptionFactory = default)
        {
            return client.SendAsObject<T>(HttpMethod.Delete, baseUrl, path, header, param, body, encoding, jsonOptions, checkStatusCode, businessExceptionFactory);
        }

        public static Task<string> PostAsString(this HttpClient client, string baseUrl, string path, object header = default, object param = default, object body = default, Encoding encoding = default, JsonSerializerOptions jsonOptions = null, bool checkStatusCode = true)
        {
            return client.SendAsString(HttpMethod.Post, baseUrl, path, header, param, body, encoding, jsonOptions, checkStatusCode);
        }
        public static async Task<T> PostAsObject<T>(this HttpClient client, string baseUrl, string path, object header = default, object param = default, object body = default, Encoding encoding = default, JsonSerializerOptions jsonOptions = null, bool checkStatusCode = true, Func<T, string, Exception> businessExceptionFactory = default)
        {
            var content = await client.PostAsString(baseUrl, path, header, param, body, encoding, jsonOptions, checkStatusCode);
            var res = content.AsJsonObject<T>(jsonOptions ?? DefaultJsonOptions);
            var businessException = businessExceptionFactory?.Invoke(res, content);
            if (businessException != null)
            {
                throw businessException;
            }
            return res;
        }
        public static Task<string> PutAsString(this HttpClient client, string baseUrl, string path, object header = default, object param = default, object body = default, Encoding encoding = default, JsonSerializerOptions jsonOptions = null, bool checkStatusCode = true)
        {
            return client.SendAsString(HttpMethod.Put, baseUrl, path, header, param, body, encoding, jsonOptions, checkStatusCode);

        }
        public static Task<T> PutAsObject<T>(this HttpClient client, string baseUrl, string path, object header = default, object param = default, object body = default, Encoding encoding = default, JsonSerializerOptions jsonOptions = null, bool checkStatusCode = true, Func<T, string, Exception> businessExceptionFactory = default)
        {
            return client.SendAsObject<T>(HttpMethod.Put, baseUrl, path, header, param, body, encoding, jsonOptions, checkStatusCode, businessExceptionFactory);
        }
        public static Task<string> PatchAsString(this HttpClient client, string baseUrl, string path, object header = default, object param = default, object body = default, Encoding encoding = default, JsonSerializerOptions jsonOptions = null, bool checkStatusCode = true)
        {
            return client.SendAsString(HttpMethod.Patch, baseUrl, path, header, param, body, encoding, jsonOptions, checkStatusCode);

        }
        public static Task<T> PatchAsObject<T>(this HttpClient client, string baseUrl, string path, object header = default, object param = default, object body = default, Encoding encoding = default, JsonSerializerOptions jsonOptions = null, bool checkStatusCode = true, Func<T, string, Exception> businessExceptionFactory = default)
        {
            return client.SendAsObject<T>(HttpMethod.Patch, baseUrl, path, header, param, body, encoding, jsonOptions, checkStatusCode, businessExceptionFactory);

        }

        static IEnumerable<KeyValuePair<string, string>> Foreach(object obj)
        {
            return ForeachAsObj(obj).Select(p => new KeyValuePair<string, string>(p.Key, FormatValue(p.Value)));
        }
        static IEnumerable<KeyValuePair<string, object>> ForeachAsObj(object obj)
        {
            if (obj != null)
            {
                if (obj is IDictionary dic)
                {
                    foreach (var key in dic.Keys)
                    {
                        yield return new KeyValuePair<string, object>($"{key}", dic[key]);
                    }
                }
                else
                {
                    foreach (var p in obj.GetType().GetProperties())
                    {
                        if (p.CanRead)
                        {
                            var val = p.GetValue(obj);
                            yield return new KeyValuePair<string, object>(p.Name, val);
                        }
                    }
                }
            }
        }
        static string FormatValue(object value)
        {
            if (value is string str)
            {
                return str;
            }
            else if (value is DateTime time)
            {
                return time.ToString("yyyy-MM-ddTHH:mm:ss.fff");
            }
            else if (value is DateTimeOffset dto)
            {
                return dto.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz");
            }
            else
            {
                return value?.ToString() ?? string.Empty;
            }
        }

        public static Task<string> PostFormAsString(this HttpClient client, string baseUrl, string path, object header = default, object param = default, object form = default, Encoding encoding = default, JsonSerializerOptions jsonOptions = null, bool checkStatusCode = true)
        {
            var formBody = CreateFormDataContent(form, encoding);
            return client.PostAsString(baseUrl, path, header, param, formBody, encoding, jsonOptions, checkStatusCode);
        }
        public static Task<T> PostFormAsObject<T>(this HttpClient client, string baseUrl, string path, object header = default, object param = default, object form = default, Encoding encoding = default, JsonSerializerOptions jsonOptions = null, bool checkStatusCode = true, Func<T, string, Exception> businessExceptionFactory = default)
        {
            var formBody = CreateFormDataContent(form, encoding);
            return client.PostAsObject<T>(baseUrl, path, header, param, formBody, encoding, jsonOptions, checkStatusCode, businessExceptionFactory);
        }
        private static HttpContent CreateFormDataContent(object form, Encoding encoding)
        {
            var multiPart = new MultipartFormDataContent();
            foreach (var (key, value) in ForeachAsObj(form))
            {
                // 字符串虽然是 IEnumerable<char>，但应视为单一值，  所以先排除 string 和 byte[]
                if (value is not string && value is not byte[] && value is IEnumerable enumerable)
                {
                    foreach (var item in enumerable)
                    {
                        AddItem(multiPart, key, item, encoding);
                    }
                }
                else
                {
                    AddItem(multiPart, key, value, encoding);
                }
            }

            return multiPart;
            static void AddItem(MultipartFormDataContent multiPart, string key, object value, Encoding encoding)
            {
                if (value is StreamBody sb)
                {
                    multiPart.Add(new StreamContent(sb.Stream), key, sb.FileName);
                }
                else if (value is Stream st)
                {
                    multiPart.Add(new StreamContent(st), key);
                }
                else if (value is byte[] by)
                {
                    multiPart.Add(new ByteArrayContent(by), key);
                }
                else
                {
                    multiPart.Add(new StringContent(FormatValue(value), encoding), key);
                }
            }
        }
    }
}

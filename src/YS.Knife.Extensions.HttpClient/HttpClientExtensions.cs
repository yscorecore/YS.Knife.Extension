using System.Collections;
using System.Data;
using System.Net.Mime;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

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
                if (body is HttpContent)
                {
                    request.Content = (HttpContent)body;
                }
                else
                {
                    request.Content = new StringContent(body.ToJsonText(DefaultJsonOptions), encoding: encoding ?? Encoding.UTF8, mediaType: MediaTypeNames.Application.Json);
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


        static IEnumerable<KeyValuePair<string, string>> Foreach(object obj)
        {
            if (obj != null)
            {
                if (obj is IDictionary dic)
                {
                    foreach (var key in dic.Keys)
                    {
                        yield return new KeyValuePair<string, string>($"{key}", FormatValue(dic[key]));
                    }
                }
                else
                {
                    foreach (var p in obj.GetType().GetProperties())
                    {
                        if (p.CanRead)
                        {
                            var val = p.GetValue(obj);
                            yield return new KeyValuePair<string, string>(p.Name, FormatValue(val));
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
    }
}

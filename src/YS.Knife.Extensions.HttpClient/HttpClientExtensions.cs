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
        private static JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };


        public static async Task<string> SendAsString(this HttpClient client, HttpMethod method, string baseUrl, string path, object header, object param, object body = default, Encoding encoding = default)
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
                    request.Content = new StringContent(body.ToJsonText(JsonOptions), encoding: encoding ?? Encoding.UTF8, mediaType: MediaTypeNames.Application.Json);
                }
            }
            var response = await client.SendAsync(request);

            response.EnsureSuccessStatusCode();
            var content = response.Content;
            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream, encoding ?? Encoding.UTF8);
            return await reader.ReadToEndAsync();

        }

        public static Task<string> GetAsString(this HttpClient client, string baseUrl, string path, object header, object param, object body = default, Encoding encoding = default)
        {
            return client.SendAsString(HttpMethod.Get, baseUrl, path, header, param, body, encoding);

        }
        public static async Task<T> GetAsObject<T>(this HttpClient client, string baseUrl, string path, object header, object param, object body = default, Encoding encoding = default)
        {
            var content = await client.GetAsString(baseUrl, path, header, param, body, encoding);
            return content.AsJsonObject<T>(JsonOptions);

        }
        public static Task<string> PostAsString(this HttpClient client, string baseUrl, string path, object header, object param = default, object body = default, Encoding encoding = default)
        {
            return client.SendAsString(HttpMethod.Post, baseUrl, path, header, param, body, encoding);

        }
        public static async Task<T> PostAsObject<T>(this HttpClient client, string baseUrl, string path, object header, object param = default, object body = default, Encoding encoding = default)
        {
            var content = await client.PostAsString(baseUrl, path, header, param, body, encoding);
            return content.AsJsonObject<T>(JsonOptions);

        }

        static IEnumerable<KeyValuePair<string, string>> Foreach(object obj)
        {
            if (obj != null)
            {
                if (obj is IDictionary dic)
                {
                    foreach (var key in dic.Keys)
                    {
                        yield return new KeyValuePair<string, string>($"{key}", $"{dic[key]?.ToString() ?? string.Empty}");
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
            else
            {
                return value?.ToString() ?? string.Empty;
            }
        }
    }
}

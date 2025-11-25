using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Playground
{
    public class DeepSeekHttpClient
    {
        private readonly HttpClient _httpClient;
        private const string ApiEndpoint = "https://api.deepseek.com/v1/chat/completions";
        public DeepSeekHttpClient(string apiKey)
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(1000);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        }
        public async Task<string> GetCompletionAsync(DeepSeekMessage[] messages)
        {
            var requestBody = new
            {
                model = "deepseek-chat",
                messages = messages
            };
            var response = await _httpClient.PostAsJsonAsync(ApiEndpoint, requestBody);
           // response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

      
    }
    public class DeepSeekMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = "user";
        [JsonPropertyName("content")]
        public string Content { get; set; }

        public static DeepSeekMessage CreateSystemMessage(string message)
        {
            return new DeepSeekMessage
            {
                Role = "system",
                Content = message,
            };
        }
        public static DeepSeekMessage CreateUserMessage(string message)
        {
            return new DeepSeekMessage
            {
                Role = "user",
                Content = message,
            };
        }
    }
}

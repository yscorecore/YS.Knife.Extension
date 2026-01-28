using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using YS.Knife.WebHooks;

namespace YS.Knife.Webhooks.Impl.Default
{
    [Service]
    [AutoConstructor]
    public partial class WebHookDeliveryService : IWebHookDeliveryService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<WebHookDeliveryService> logger;
        public async Task SendEvent<T>(WebhookEvent<T> webhookEvent, WebhookConfig config, CancellationToken token = default)
        {
            var encoding = config.Encoding ?? Encoding.UTF8;
            var jsonContent =
                config.EventDataOnly ? JsonSerializer.Serialize(webhookEvent.Data, config.JsonOptions) :
              JsonSerializer.Serialize(webhookEvent, config.JsonOptions);

            var timeStamp = webhookEvent.EventTime.ToUnixTimeMilliseconds();
            var request = new HttpRequestMessage(HttpMethod.Post, config.CallbackUrl)
            {
                Content = new StringContent(jsonContent, encoding, MediaTypeNames.Application.Json)
            };

            var signature = GenerateSignature($"{timeStamp}.{jsonContent}", encoding, config.SecretKey);
            request.Headers.Add("X-Webhook-EventId", webhookEvent.EventId);
            request.Headers.Add("X-Webhook-EventType", webhookEvent.EventType);
            request.Headers.Add("X-Webhook-EventTime", timeStamp.ToString());
            request.Headers.Add("X-Webhook-Signature", signature);
            var response = await _httpClient.SendAsync(request, token);
            var responseReasonPhrase = response.ReasonPhrase;
            var responseText = await response.Content.ReadAsStringAsync(token);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogInformation(
                 "Webhook delivery successful. - EventId: {EventId}, URL: {Url}, StatusCode: {StatusCode}",
                 webhookEvent.EventId,
                 config.CallbackUrl,
                 (int)response.StatusCode);
            }
            else
            {
                logger.LogInformation(
                "Webhook delivery failed. - EventId: {EventId}, URL: {Url}, RequestBody: {RequestBody} StatusCode: {StatusCode} ReasonPhrase {ReasonPhrase} ResponseBody: {ResponseBody}",
                webhookEvent.EventId,
                config.CallbackUrl,
                jsonContent,
                (int)response.StatusCode,
                responseReasonPhrase,
                responseText
                );
                throw new WebhookDeliveryException(
                     $"Webhook delivery failed: {response.StatusCode} - {response.ReasonPhrase}",
                     (int)response.StatusCode,
                     responseText)
                {
                    EventId = webhookEvent.EventId,
                    Url = config.CallbackUrl
                };
            }
        }

        /// <summary>
        /// 生成HMAC SHA256签名（核心安全机制）
        /// </summary>
        private string GenerateSignature(string payload, Encoding encoding, string secretKey)
        {
            using var hmac = new HMACSHA256(encoding.GetBytes(secretKey));
            var hash = hmac.ComputeHash(encoding.GetBytes(payload));
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }

    public class WebhookDeliveryException : Exception
    {
        public string EventId { get; set; } = null!;
        public string Url { get; set; } = null!;
        public int? StatusCode { get; set; }
        public string ResponseContent { get; set; } = null!;

        public WebhookDeliveryException(string message) : base(message)
        {
        }

        public WebhookDeliveryException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public WebhookDeliveryException(string message, int statusCode, string responseContent)
            : base(message)
        {
            StatusCode = statusCode;
            ResponseContent = responseContent;
        }

        public WebhookDeliveryException(string message, int statusCode, string responseContent, Exception innerException)
            : base(message, innerException)
        {
            StatusCode = statusCode;
            ResponseContent = responseContent;
        }
    }
}

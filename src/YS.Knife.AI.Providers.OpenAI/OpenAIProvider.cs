using System;
using System.ClientModel;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using OpenAI;
using OpenAI.Chat;
using YS.Knife.AI.Core;

namespace YS.Knife.AI.Providers.OpenAI
{
    [Options]
    public class OpenAIOptions
    {
        [Required]
        public string ApiKey { get; set; } = string.Empty;
        public string? BaseUrl { get; set; }
        public string? Organization { get; set; }
    }

    [Service]
    [AutoConstructor]
    [DictionaryKey("openai")]
    public partial class OpenAIProvider : IAiProviderService
    {
        [AutoConstructorIgnore]
        private OpenAIClient _client;

        private readonly OpenAIOptions _options;

        [AutoConstructorInitialize]
        private void InitClient()
        {
            var options = this._options;
            ApiKeyCredential credential = new ApiKeyCredential(options.ApiKey);

            if (!string.IsNullOrWhiteSpace(options.BaseUrl))
            {
                // 使用自定义 base URL
                OpenAIClientOptions clientOptions = new OpenAIClientOptions
                {
                    Endpoint = new Uri(options.BaseUrl),
                };

                _client = new OpenAIClient(credential, clientOptions);
            }
            else
            {
                // 使用默认的 OpenAI base URL
                _client = new OpenAIClient(credential);
            }
        }

        public async Task<string> RecognizeImage(string imageUrlOrBase64, string model, string prompt, CancellationToken cancellationToken = default)
        {
            var imageSource = DetermineImageSource(imageUrlOrBase64);
            ChatClient chatClient = _client.GetChatClient(model);

            List<ChatMessage> messages = new List<ChatMessage>();

            if (imageSource == ImageSource.Base64)
            {
                // Remove data:image/...;base64, prefix if present
                string base64Data = imageUrlOrBase64;
                if (base64Data.Contains(','))
                {
                    base64Data = base64Data.Split(',')[1];
                }

                messages.Add(new UserChatMessage(
                    ChatMessageContentPart.CreateImagePart(BinaryData.FromBytes(Convert.FromBase64String(base64Data)), "image/png"),
                    ChatMessageContentPart.CreateTextPart(prompt)));
            }
            else
            {
                messages.Add(new UserChatMessage(
                    ChatMessageContentPart.CreateImagePart(new Uri(imageUrlOrBase64)),
                    ChatMessageContentPart.CreateTextPart(prompt)));
            }

            ClientResult<ChatCompletion> result = await chatClient.CompleteChatAsync(messages, cancellationToken: cancellationToken);
            return result.Value.Content[0].Text;
        }

        private static ImageSource DetermineImageSource(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                throw new ArgumentException("Image input cannot be null or empty.", nameof(input));
            }

            // Base64 images typically have data:image/...;base64,... format or just base64 string
            if (input.StartsWith("data:", StringComparison.OrdinalIgnoreCase) ||
                (input.Length > 100 && !input.Contains("://") && IsBase64String(input)))
            {
                return ImageSource.Base64;
            }

            return ImageSource.Url;
        }

        private static bool IsBase64String(string input)
        {
            try
            {
                Convert.FromBase64String(input);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private enum ImageSource
        {
            Url,
            Base64
        }
    }
}

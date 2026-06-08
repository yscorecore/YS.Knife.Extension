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

        public async Task<string> RecognizeImage(AiInputData[] inputs, string model, string prompt, CancellationToken cancellationToken = default)
        {
            ChatClient chatClient = _client.GetChatClient(model);
            List<ChatMessage> messages = new List<ChatMessage>
            {
                new UserChatMessage(inputs.Select(ConvertToConent).ConcatItems(ChatMessageContentPart.CreateTextPart(prompt)))
            };
            ClientResult<ChatCompletion> result = await chatClient.CompleteChatAsync(messages, cancellationToken: cancellationToken);
            return result.Value.Content[0].Text;
        }

        ChatMessageContentPart ConvertToConent(AiInputData input)
        {
            var mediaType = input.MediaType;
            return input.Type switch
            {
                AiDataType.Stream => ChatMessageContentPart.CreateImagePart(BinaryData.FromStream((Stream)input.Data), input.MediaType),
                AiDataType.Url => ChatMessageContentPart.CreateImagePart(new Uri((string)input.Data)),
                AiDataType.Base64 => ChatMessageContentPart.CreateImagePart(BinaryData.FromBytes(Convert.FromBase64String((string)input.Data)), input.MediaType),
                AiDataType.ByteArray => ChatMessageContentPart.CreateImagePart(BinaryData.FromBytes((byte[])input.Data), input.MediaType),
                _ => throw new NotSupportedException($"Unsupported AiDataType: {input.Type}")
            };

        }



    }
}

using System;
using System.ClientModel;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using OpenAI;
using OpenAI.Chat;
using YS.Knife.AI.Core;

namespace YS.Knife.AI.Providers.OpenAI
{

    internal interface IProviderOptions
    {
        string ApiKey { get; set; }
        string? BaseUrl { get; set; }
        string[]? Models { get; set; }
        string? Organization { get; set; }
    }

    [Options]
    public class ProviderOptions : IProviderOptions
    {
        [Required]
        public string ApiKey { get; set; } = string.Empty;
        public string? BaseUrl { get; set; }
        public string? Organization { get; set; }
        public string[]? Models { get; set; } = null;
    }
    [Options]
    public class OpenAIOptions : Dictionary<string, ProviderOptions>, IProviderOptions
    {
        public string ApiKey { get; set; } = string.Empty;
        public string? BaseUrl { get; set; }
        public string? Organization { get; set; }
        public string[]? Models { get; set; } = null;
    }

    [Service]
    [AutoConstructor]
    [DictionaryKey("openai")]
    [CodeExceptions]
    public partial class OpenAIProvider : IAiProviderService
    {

        private readonly OpenAIOptions openAIOptions;
        [CodeException("AI002", "Can not find section: '{name}'")]
        internal partial Exception CanNotFindSection(string name);
        private (OpenAIClient, ChatClient) InitClient(string modelName)
        {
            var index = modelName.IndexOf("/");
            if (index <= 0)
            {
                return CreateClient(modelName, this.openAIOptions);
            }
            else
            {
                var provider = modelName[..index];
                var model = modelName[(index + 1)..];
                if (this.openAIOptions.TryGetValue(provider, out var options))
                {
                    return CreateClient(model, options);
                }
                else
                {
                    throw CanNotFindSection(provider);
                }
            }
            (OpenAIClient, ChatClient) CreateClient(string modelName, IProviderOptions options)
            {
                var credential = new ApiKeyCredential(options.ApiKey);
                if (!string.IsNullOrWhiteSpace(options.BaseUrl))
                {
                    // 使用自定义 base URL
                    OpenAIClientOptions clientOptions = new OpenAIClientOptions
                    {
                        Endpoint = new Uri(options.BaseUrl),
                    };

                    var openAiClient = new OpenAIClient(credential, clientOptions);
                    return (openAiClient, openAiClient.GetChatClient(modelName));
                }
                else
                {
                    // 使用默认的 OpenAI base URL
                    var openAiClient = new OpenAIClient(credential);
                    return (openAiClient, openAiClient.GetChatClient(modelName));
                }
            }

        }


        public async Task<string> RecognizeImage(AiInputData[] inputs, string modelName, string prompt, CancellationToken cancellationToken = default)
        {
            var (openaiClient, chatClient) = InitClient(modelName);
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

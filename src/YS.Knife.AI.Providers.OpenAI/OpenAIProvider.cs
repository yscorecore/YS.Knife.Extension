using System;
using System.ClientModel;
using System.ClientModel.Primitives;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Files;
using YS.Knife.AI.Core;

namespace YS.Knife.AI.Providers.OpenAI
{

    internal interface IProviderOptions
    {
        string ApiKey { get; }
        string? BaseUrl { get; }
        string[]? Models { get; }
        string? Organization { get; }

        string UploadFilePurpose { get; }
    }

    [Options]
    public class ProviderOptions : IProviderOptions
    {
        [Required]
        public string ApiKey { get; set; } = string.Empty;
        public string? BaseUrl { get; set; }
        public string? Organization { get; set; }
        public string[]? Models { get; set; } = null;
        public string UploadFilePurpose { get; set; } = "user_data";
    }
    [Options]
    public class OpenAIOptions : Dictionary<string, ProviderOptions>, IProviderOptions
    {
        public string ApiKey { get; set; } = string.Empty;
        public string? BaseUrl { get; set; }
        public string? Organization { get; set; }
        public string[]? Models { get; set; } = null;

        public string UploadFilePurpose { get; set; } = "user_data";
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
        private (OpenAIClient, ChatClient, IProviderOptions) InitClient(string modelName)
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
            (OpenAIClient, ChatClient, IProviderOptions) CreateClient(string modelName, IProviderOptions options)
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
                    return (openAiClient, openAiClient.GetChatClient(modelName), options);
                }
                else
                {
                    // 使用默认的 OpenAI base URL
                    var openAiClient = new OpenAIClient(credential);
                    return (openAiClient, openAiClient.GetChatClient(modelName), options);
                }
            }

        }


        public async Task<string> RecognizeImage(AiInputData[] inputs, string modelName, string prompt, CancellationToken cancellationToken = default)
        {
            var (openaiClient, chatClient, options) = InitClient(modelName);
            List<ChatMessage> messages = new List<ChatMessage>
            {
                new UserChatMessage(inputs.Select(ConvertToConent).ConcatItems(ChatMessageContentPart.CreateTextPart(prompt)))
            };
            ClientResult<ChatCompletion> result = await chatClient.CompleteChatAsync(messages, cancellationToken: cancellationToken);
            return result.Value.Content[0].Text;
        }
        public async Task<string> Chat(string modelName, string prompt, CancellationToken cancellationToken = default)
        {
            var (openaiClient, chatClient, options) = InitClient(modelName);
            List<ChatMessage> messages = new List<ChatMessage>
            {
                new UserChatMessage(ChatMessageContentPart.CreateTextPart(prompt))
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

        public async Task<string> RecognizeDocument(AiInputData[] inputs, string modelName, string prompt, CancellationToken cancellationToken = default)
        {
            var (openaiClient, chatClient, options) = InitClient(modelName);
            var fileClient = openaiClient.GetOpenAIFileClient();
            var files = await UploadFiles(fileClient, inputs, options);
            var fileReferences = string.Join("\n", files.Select(id => $"fileid://{id}"));

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(fileReferences),
                new UserChatMessage(prompt)
            };
            ClientResult<ChatCompletion> result = await chatClient.CompleteChatAsync(messages, cancellationToken: cancellationToken);
            return result.Value.Content[0].Text;
        }

        async Task<string[]> UploadFiles(OpenAIFileClient client, AiInputData[] inputs, IProviderOptions options)
        {
            var res = new List<string>();
            foreach (var input in inputs)
            {
                res.Add(await UploadFile(client, input, options));
            }
            return res.ToArray();
        }
        async Task<string> UploadFile(OpenAIFileClient client, AiInputData input, IProviderOptions options)
        {
            (Stream fileStream, string fileName, string? mediaType) = input.Type switch
            {
                AiDataType.Url => await PrepareFromUrlAsync(input),
                AiDataType.ByteArray => PrepareFromBytes(input),
                AiDataType.Base64 => PrepareFromBase64(input),
                AiDataType.Stream => PrepareFromStream(input),
                _ => throw new NotSupportedException($"Unsupported AiDataType: {input.Type}")
            };
            try
            {
                return await UploadFileByInternalApi(client, fileStream, fileName, input.MediaType, options);
            }
            finally
            {
                if (input.Type != AiDataType.Stream && fileStream != null)
                {
                    await fileStream.DisposeAsync();
                }
            }
            async Task<string> UploadFileByInternalApi(OpenAIFileClient client, Stream fileStream, string fileName, string? mediaType, IProviderOptions options)
            {
                var boundary = "JRTaGGD35vdOu1wjW6Rou71qWybpyp_qs3F4kWkJMWMJY_Cy3RQkJAplASJ9L_ElHI853W";
                var contentType = $"multipart/form-data; boundary=\"{boundary}\"";
                var multiPart = new MultipartFormDataContent(boundary);


                multiPart.Add(new StreamContent(fileStream), "file", fileName);

                // Set the purpose
                multiPart.Add(new StringContent(options.UploadFilePurpose, Encoding.UTF8), "purpose");

                // Set the expiration (don't be mislead by the "object" designation in the platform docs, they're content parts)
                //  multiPart.Add(new StringContent("created_at", Encoding.UTF8), "expires_after[anchor]");
                // multiPart.Add(new StringContent("3600", Encoding.UTF8), "expires_after[seconds]");

                // Create the client and send the request.
                using var ms = await multiPart.ReadAsStreamAsync();
                var rawResponse = await client.UploadFileAsync(BinaryContent.Create(ms), contentType);

                var bin = rawResponse.GetRawResponse().Content;
                var f1 = bin.ToObjectFromJson<Dictionary<string, object>>();

                return f1!.TryGetValue("id", out var obj) ? Convert.ToString(obj)! : throw new Exception("Upload file error");
            }

            async Task<(Stream, string, string?)> PrepareFromUrlAsync(AiInputData input)
            {
                string url = (string)input.Data;
                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                // 获取文件名：从 URL 提取最后一段，或从 Content-Disposition 获取
                string fileName = GenerateFileName(response.Content.Headers.ContentType?.MediaType);

                var memoryStream = new MemoryStream();
                await response.Content.CopyToAsync(memoryStream);
                memoryStream.Position = 0; // 重置位置供读取

                return (memoryStream, fileName, response.Content.Headers.ContentType?.MediaType);
            }

            // 从字节数组准备
            static (Stream, string, string?) PrepareFromBytes(AiInputData input)
            {
                byte[] bytes = (byte[])input.Data;
                var stream = new MemoryStream(bytes);
                string fileName = GenerateFileName(input.MediaType);
                return (stream, fileName, input.MediaType);
            }

            // 从 Base64 字符串准备
            static (Stream, string, string?) PrepareFromBase64(AiInputData input)
            {
                string base64String = (string)input.Data;
                byte[] bytes = Convert.FromBase64String(base64String);
                var stream = new MemoryStream(bytes);
                string fileName = GenerateFileName(input.MediaType);
                return (stream, fileName, input.MediaType);
            }

            // 从外部流准备（不释放流）
            static (Stream, string, string?) PrepareFromStream(AiInputData input)
            {
                Stream stream = (Stream)input.Data;
                // 确保流位置在开头（如果可 Seek）
                if (stream.CanSeek && stream.Position != 0)
                {
                    stream.Seek(0, SeekOrigin.Begin);
                }
                string fileName = GenerateFileName(input.MediaType);
                return (stream, fileName, input.MediaType);
            }



            // 根据媒体类型生成文件名
            static string GenerateFileName(string? mediaType)
            {
                return $"file_{Guid.NewGuid():N}{ContentTypeMappings.GetExtNameByContentType(mediaType)}";
            }
        }


    }
}




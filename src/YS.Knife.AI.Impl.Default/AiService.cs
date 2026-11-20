using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using YS.Knife.AI.Core;

namespace YS.Knife.AI.Impl.Default
{
    [YS.Knife.Service]
    [AutoConstructor]
    [CodeExceptions]
    public partial class AiService : IAiService
    {
        private readonly IDictionary<string, IAiProviderService> providers;
        private readonly ILogger<AiService> logger;
        private readonly ClassDefinitionGenerator classDefinitionGenerator;
        private static readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        [CodeException("AI002", "Invalid provider name: '{name}'. Please use the format provider-name::model-name (e.g., openai::gpt-4, anthropic::claude-3")]
        private partial Exception InvalidModelProviderName(string name);
        [CodeException("AI001", "AI provider {name} not found.")]
        private partial Exception CreateProviderNotFoundException(string name);
        public async Task<T> RecognizeImageAsObject<T>(AiInputData[] inputs, string modelProviderName, string prompt, CancellationToken cancellationToken = default)
             where T : class, new()
        {
            var (provider, model) = ParseModelName(modelProviderName);
            if (providers.TryGetValue(provider, out var providerService))
            {
                var fullPrompt = GetFullPromptForObject<T>(prompt, model);
                logger.LogInformation("Recognizing image with provider {provider}, model {model}, prompt {prompt}", provider, model, fullPrompt);
                var content = await providerService.RecognizeImage(inputs, model, fullPrompt, cancellationToken);
                logger.LogInformation("Received response from provider {provider}: {content}", provider, content);
                return ParseResultJson<T>(content);
            }
            else
            {
                throw CreateProviderNotFoundException(provider);
            }
        }
        private (string Provider, string Model) ParseModelName(string modelProviderName)
        {
            var index = modelProviderName.IndexOf("::");
            if (index <= 0)
            {
                throw InvalidModelProviderName(modelProviderName);
            }
            else
            {
                var provider = modelProviderName[..index];
                var modelName = modelProviderName[(index + 2)..];
                return (provider, modelName);
            }
        }
        public async Task<T[]> RecognizeImageAsArray<T>(AiInputData[] inputs, string modelProviderName, string prompt, CancellationToken cancellationToken = default)
             where T : class, new()
        {
            var (provider, model) = ParseModelName(modelProviderName);
            if (providers.TryGetValue(provider, out var providerService))
            {
                var fullPrompt = GetFullPromptForArray<T>(prompt, model);
                logger.LogInformation("Recognizing image with provider {provider}, model {model}, prompt {prompt}", provider, model, fullPrompt);
                var content = await providerService.RecognizeImage(inputs, model, fullPrompt, cancellationToken);
                logger.LogInformation("Received response from provider {provider}: {content}", provider, content);
                return ParseResultJson<T[]>(content);
            }
            else
            {
                throw CreateProviderNotFoundException(provider);
            }
        }
        private string GetFullPromptForObject<T>(string prompt, string model)
        {
            return @$"{prompt}
---------------
不需要思考过程，直接输出结果，要求输出单个对象的json字符串，json字符串要能被反序列化为以下c#类型，字段名称使用小驼峰命名法。
```csharp
{classDefinitionGenerator.GetClassDefinition(typeof(T))}
```
";
        }
        private string GetFullPromptForArray<T>(string prompt, string model)
        {
            return @$"{prompt}
---------------
不需要思考过程，直接输出结果，要求输出json对象的数组的字符串，json字符串要能被反序列化为以下c#类型的数组，字段名称使用小驼峰命名法。
```csharp
{classDefinitionGenerator.GetClassDefinition(typeof(T))}
```
";
        }
        private T ParseResultJson<T>(string content)
        {
            var validJson = content.Trim();
            if (content.StartsWith("```json") && content.EndsWith("```"))
            {
                validJson = validJson.Substring(7, content.Length - 10).Trim();
            }
            return validJson.AsJsonObject<T>(jsonSerializerOptions);
        }

        public async Task<T> RecognizeDocumentAsObject<T>(AiInputData[] inputs, string modelProviderName, string prompt, CancellationToken cancellationToken = default) where T : class, new()
        {
            var (provider, model) = ParseModelName(modelProviderName);
            if (providers.TryGetValue(provider, out var providerService))
            {
                var fullPrompt = GetFullPromptForObject<T>(prompt, model);
                logger.LogInformation("Recognizing document with provider {provider}, model {model}, prompt {prompt}", provider, model, fullPrompt);
                var content = await providerService.RecognizeDocument(inputs, model, fullPrompt, cancellationToken);
                logger.LogInformation("Received response from provider {provider}: {content}", provider, content);
                return ParseResultJson<T>(content);
            }
            else
            {
                throw CreateProviderNotFoundException(provider);
            }
        }

        public async Task<T[]> RecognizeDocumentAsArray<T>(AiInputData[] inputs, string modelProviderName, string prompt, CancellationToken cancellationToken = default) where T : class, new()
        {
            var (provider, model) = ParseModelName(modelProviderName);
            if (providers.TryGetValue(provider, out var providerService))
            {
                var fullPrompt = GetFullPromptForArray<T>(prompt, model);
                logger.LogInformation("Recognizing document with provider {provider}, model {model}, prompt {prompt}", provider, model, fullPrompt);
                var content = await providerService.RecognizeDocument(inputs, model, fullPrompt, cancellationToken);
                logger.LogInformation("Received response from provider {provider}: {content}", provider, content);
                return ParseResultJson<T[]>(content);
            }
            else
            {
                throw CreateProviderNotFoundException(provider);
            }
        }

        public async Task<T> ChatAsObject<T>(string modelProviderName, string prompt, CancellationToken cancellationToken = default) where T : class, new()
        {
            var (provider, model) = ParseModelName(modelProviderName);
            if (providers.TryGetValue(provider, out var providerService))
            {
                var fullPrompt = GetFullPromptForObject<T>(prompt, model);
                logger.LogInformation("Chat with provider {provider}, model {model}, prompt {prompt}", provider, model, fullPrompt);
                var content = await providerService.Chat(model, fullPrompt, cancellationToken);
                logger.LogInformation("Received response from provider {provider}: {content}", provider, content);
                return ParseResultJson<T>(content);
            }
            else
            {
                throw CreateProviderNotFoundException(provider);
            }
        }

        public async Task<T[]> ChatAsArray<T>(string modelProviderName, string prompt, CancellationToken cancellationToken = default) where T : class, new()
        {
            var (provider, model) = ParseModelName(modelProviderName);
            if (providers.TryGetValue(provider, out var providerService))
            {
                var fullPrompt = GetFullPromptForArray<T>(prompt, model);
                logger.LogInformation("Chat with provider {provider}, model {model}, prompt {prompt}", provider, model, fullPrompt);
                var content = await providerService.Chat(model, fullPrompt, cancellationToken);
                logger.LogInformation("Received response from provider {provider}: {content}", provider, content);
                return ParseResultJson<T[]>(content);
            }
            else
            {
                throw CreateProviderNotFoundException(provider);
            }
        }
    }
}

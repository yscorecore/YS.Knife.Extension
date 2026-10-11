# YS.Knife.AI.Providers.OpenAI

基于官方 OpenAI .NET SDK 的 AI 服务提供者实现，支持文本对话、图片识别与文档识别，可通过字典配置接入多个 OpenAI 兼容的模型端点。

## 主要功能

- `OpenAIProvider`：实现 `IAiProviderService`，以字典键 `openai` 注册，提供：
  - `Chat(modelName, prompt)`：纯文本对话。
  - `RecognizeImage(inputs, modelName, prompt)`：图片识别，输入支持 Stream / Url / Base64 / 字节数组四种形式（`AiDataType`）。
  - `RecognizeDocument(inputs, modelName, prompt)`：先将文件上传到 OpenAI Files API，再以 `fileid://` 引用方式发起文档理解请求。
- 支持自定义 `BaseUrl`，可接入 OpenAI 兼容的第三方服务（如 Azure 代理、私有化模型网关等）。
- `OpenAIOptions`：字典型配置，除顶层默认配置外，可按“提供者名”配置多套 `ProviderOptions`；模型名使用 `provider/model` 格式时会路由到对应配置节，找不到时抛出 `AI002` 代码异常。
- `ProviderOptions`：单个提供者的配置，包含 `ApiKey`、`BaseUrl`、`Organization`、`Models`、`UploadFilePurpose`（默认 `user_data`）。

## 使用示例

```json
{
  "OpenAIOptions": {
    "ApiKey": "sk-default",
    "Azure": {
      "ApiKey": "sk-azure",
      "BaseUrl": "https://my-gateway.example.com/v1"
    }
  }
}
```

```csharp
public class VisionService
{
    private readonly IAiProviderService ai; // 通过字典键 "openai" 解析

    public async Task<string> DescribeAsync(byte[] imageBytes)
    {
        var input = new AiInputData
        {
            Type = AiDataType.ByteArray,
            Data = imageBytes,
            MediaType = "image/png"
        };
        // 使用 "Azure/gpt-4o" 可路由到上面配置的 Azure 节
        return await ai.RecognizeImage(new[] { input }, "gpt-4o", "描述这张图片的内容");
    }
}
```

## 依赖说明

- `OpenAI`（2.11.0）：官方 OpenAI .NET SDK。
- `YS.Knife.AI.Core`：提供 `IAiProviderService`、`AiInputData`、`AiDataType` 等抽象。
- `YS.Knife.Core`：提供 `[Service]`、`[DictionaryKey]`、`[Options]`、`[CodeExceptions]` 等源生成特性。

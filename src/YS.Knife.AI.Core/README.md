# YS.Knife.AI.Core

AI 服务的抽象层，定义了强类型结果（对象/数组）的图像识别、文档识别与文本对话接口，以及输入数据模型。

## 主要功能

- `IAiService`：面向业务的 AI 接口，结果直接反序列化为指定类型
  - `RecognizeImageAsObject<T>` / `RecognizeImageAsArray<T>`
  - `RecognizeDocumentAsObject<T>` / `RecognizeDocumentAsArray<T>`
  - `ChatAsObject<T>` / `ChatAsArray<T>`
- `IAiProviderService`：面向具体 AI 提供商的底层接口（返回原始字符串），由 provider 实现（如 OpenAI）
  - `RecognizeImage` / `RecognizeDocument` / `Chat`
- `AiInputData`：多模态输入数据，支持四种类型（`AiDataType`：`Url`、`Base64`、`ByteArray`、`Stream`）
  - `AiInputData.From(string)`：自动识别 http(s) URL、本地文件路径、`data:` base64 字符串、纯 base64 字符串
  - `AiInputData.FromStream` / `FromBytes` / `FromStreamBody`：其他构造方式

## 使用示例

```csharp
// 构造输入
var inputs = new[]
{
    AiInputData.From("https://example.com/id-card.jpg"),  // URL
    AiInputData.From(@"C:\images\invoice.pdf"),           // 本地文件
    AiInputData.FromBytes(bytes, "image/png")             // 字节数组
};

// 通过 IAiService（实现见 YS.Knife.AI.Impl.Default）获取强类型结果
var result = await aiService.RecognizeImageAsObject<IdCardInfo>(
    inputs, "openai::gpt-4o", "识别身份证信息");
```

## 依赖说明

- `YS.Knife.Extensions`：`StreamBody`、`ContentTypeMappings`、base64 校验等基础工具。
- 默认编排实现见 `YS.Knife.AI.Impl.Default`；provider 实现见 `YS.Knife.AI.Providers.OpenAI` 等。

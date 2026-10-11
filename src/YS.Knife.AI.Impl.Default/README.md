# YS.Knife.AI.Impl.Default

`IAiService` 的默认实现。把 C# 类型的类定义注入提示词，驱动多模态大模型（图像/文档识别、文本对话）直接返回可反序列化为指定类型的 JSON 结果。

## 主要功能

- `AiService`：实现 `IAiService`
  - `RecognizeImageAsObject<T>` / `RecognizeImageAsArray<T>`：图像识别并返回强类型对象/数组
  - `RecognizeDocumentAsObject<T>` / `RecognizeDocumentAsArray<T>`：文档识别并返回强类型对象/数组
  - `ChatAsObject<T>` / `ChatAsArray<T>`：文本对话并返回强类型对象/数组
- 模型寻址格式：`provider-name::model-name`（如 `openai::gpt-4`、`anthropic::claude-3`），按 provider 名称路由到对应的 `IAiProviderService`
- 自动构造提示词：将目标类型的 C# 类定义（含 `Description`/`Display` 特性）附在提示词后，要求模型输出小驼峰 JSON，并兼容剥离 ```` ```json ```` 代码块
- `ClassDefinitionGenerator`：递归收集类型依赖，生成包含命名空间、特性标注的完整 C# 类定义文本
- 带编码的异常：未找到 provider 抛出 `AI001`，模型名格式错误抛出 `AI002`

## 快速开始

定义返回类型并调用：

```csharp
public class InvoiceInfo
{
    [Description("发票号码")]
    public string InvoiceNo { get; set; }

    [Description("金额")]
    public decimal Amount { get; set; }
}

public class InvoiceService
{
    private readonly IAiService aiService;

    public InvoiceService(IAiService aiService) => this.aiService = aiService;

    public async Task<InvoiceInfo> ParseInvoice(string imagePath)
    {
        var inputs = new[] { AiInputData.From(imagePath) };
        return await aiService.RecognizeImageAsObject<InvoiceInfo>(
            inputs, "openai::gpt-4o", "识别图片中的发票信息");
    }
}
```

服务通过 YS.Knife `[Service]` 特性自动注册；需同时引用一个具体的 provider 实现（如 `YS.Knife.AI.Providers.OpenAI`）提供 `IAiProviderService`。

## 依赖说明

- `YS.Knife.AI.Core`：`IAiService`、`IAiProviderService`、`AiInputData` 抽象。
- `YS.Knife.Extensions`：JSON 反序列化等扩展。
- `YS.Knife.Core`（2.0.2）：`[Service]`、`[AutoConstructor]`、`[CodeExceptions]`。

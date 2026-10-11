# YS.Knife.Tts.Api.AspnetCore

`YS.Knife.Tts` 文字转语音功能的 ASP.NET Core Web API 层，将 `ITextSpeechService` 暴露为 HTTP 接口。

## 主要功能

- 提供 `TtsController`（路由 `[controller]`，即 `/Tts`）
- `GET /Tts`：文字转语音，返回 JSON 形式的 `TextSpeechRes`
- `GET /Tts/download`：文字转语音并直接下载为 mp3 文件
- 控制器通过 `[AutoConstructor]` 自动注入 `ITextSpeechService`

## 接口说明

```csharp
[ApiController]
[Route("[controller]")]
public partial class TtsController : ControllerBase
{
    [HttpGet]
    public Task<TextSpeechRes> TextToSpeech([FromQuery] TextSpeechReq req, CancellationToken cancellationToken);

    [HttpGet]
    [Route("download")]
    public Task<IActionResult> DownLoad([FromQuery] TextSpeechReq req, CancellationToken cancellationToken);
}
```

## 使用示例

引用本包后，将控制器程序集加入 MVC 应用即可：

```csharp
builder.Services.AddControllers()
    .AddApplicationPart(typeof(TtsController).Assembly);
```

请求示例：

```http
GET /Tts?text=你好世界
GET /Tts/download?text=你好世界
```

- `/Tts` 返回：`{ "audio": "<base64 字节数组>" }`
- `/Tts/download` 返回：`audio/mpeg` 文件流，文件名形如 `yyyyMMddHHmmss_fff.mp3`

## 依赖说明

- `Microsoft.NET.Sdk.Web`（OutputType 为 Library，可打包为 NuGet）
- `YS.Knife.Tts.Core`：提供 `ITextSpeechService`、`TextSpeechReq`、`TextSpeechRes` 定义

运行时必须同时引用一个 TTS 实现包（如 `YS.Knife.Tts.Impl.Aliyun`），并在容器中注册 `ITextSpeechService`，否则接口调用将因缺少实现而失败。

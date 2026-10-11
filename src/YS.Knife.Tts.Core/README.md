# YS.Knife.Tts.Core

TTS（Text To Speech，文本转语音）功能的抽象层，定义统一的文本转语音服务接口。

## 主要功能

- 定义 `ITextSpeechService` 文本转语音服务接口
- 定义请求/响应模型 `TextSpeechReq` / `TextSpeechRes`
- 不包含任何具体实现，可替换不同的 TTS 引擎（阿里云、Azure 等）

## 快速开始

在业务代码中注入 `ITextSpeechService` 接口，具体实现由实现层包（如 `YS.Knife.Tts.Impl.Aliyun`）提供：

```csharp
using YS.Knife.Tts;

public class VoiceService
{
    private readonly ITextSpeechService textSpeechService;

    public VoiceService(ITextSpeechService textSpeechService)
    {
        this.textSpeechService = textSpeechService;
    }

    public async Task<byte[]> SpeakAsync(string text, CancellationToken cancellationToken)
    {
        var res = await textSpeechService.TextToSpeech(
            new TextSpeechReq { Text = text }, cancellationToken);
        return res.Audio;
    }
}
```

## 接口说明

```csharp
public interface ITextSpeechService
{
    Task<TextSpeechRes> TextToSpeech(TextSpeechReq req, CancellationToken cancellationToken);
}

public record TextSpeechReq
{
    public string Text { get; set; }
}

public record TextSpeechRes
{
    public byte[] Audio { get; set; }   // 合成后的音频字节流
}
```

## 依赖说明

- 目标框架：.NET 6（`net6.0`）
- 无第三方运行时依赖，仅定义抽象契约
- 实现层请参考 `YS.Knife.Tts.Impl.Aliyun`（阿里云语音合成）

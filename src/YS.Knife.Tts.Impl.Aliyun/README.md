# YS.Knife.Tts.Impl.Aliyun

基于阿里云智能语音交互服务的 `ITextSpeechService` 实现，将文本合成为 MP3 音频。

## 主要功能

- `TextSpeechService`：调用阿里云语音合成接口（`/stream/v1/tts`），返回 MP3 音频字节流
- `TokenManager`：阿里云访问令牌的自动获取与缓存（基于 `IDistributedCache` 缓存 + `ILockService` 分布式锁防止重复刷新）
- `AliyunTtsOptions`：阿里云接入配置（AccessKey、AppKey、网关地址等）

## 快速开始

服务通过 FlyTiger 的 `[Service]` 特性自动注册到依赖注入容器，使用时注入 `ITextSpeechService` 即可：

```csharp
using YS.Knife.Tts;

public class NotifyService
{
    private readonly ITextSpeechService ttsService;

    public NotifyService(ITextSpeechService ttsService)
    {
        this.ttsService = ttsService;
    }

    public async Task<byte[]> MakeVoiceAsync(CancellationToken cancellationToken)
    {
        var res = await ttsService.TextToSpeech(
            new TextSpeechReq { Text = "今天是周一，天气挺好的。" },
            cancellationToken);
        return res.Audio;   // MP3 音频数据
    }
}
```

## 配置说明

通过 `AliyunTtsOptions` 配置阿里云接入参数（需要配置 DI 容器中的 Options）：

```csharp
services.Configure<AliyunTtsOptions>(opt =>
{
    opt.AccessKeyId = "your-access-key-id";       // 必填
    opt.AccessKeySecret = "your-access-key-secret"; // 必填
    opt.AppKey = "your-app-key";                   // 必填
    opt.BaseUrl = "http://nls-gateway.cn-shanghai.aliyuncs.com"; // 默认
    opt.RegionId = "cn-shanghai";                  // 默认
});
```

## 依赖说明

- 目标框架：.NET 6（`net6.0`）
- `YS.Knife.Tts.Core`：抽象接口
- `YS.Knife.Extensions.HttpClient` / `YS.Knife.Extensions.DistributedCache` / `YS.Knife.Lock.Core`：HTTP 调用、令牌缓存与分布式锁
- `YS.Knife.Core`、`FlyTiger`：服务自动注册与代码生成

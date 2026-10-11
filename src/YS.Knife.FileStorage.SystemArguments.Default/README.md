# YS.Knife.FileStorage.SystemArguments.Default

为文件存储路径模板提供的一组内置系统参数提供者（`ISystemArgProvider` 实现），可在文件 key/路径模板中按名称引用当前时间、GUID、随机数、时间戳等动态值。

## 主要功能

内置以下参数提供者，均以 Singleton 生命周期自动注册，并通过 `[DictionaryKey]` 指定引用名称：

| 名称 | 提供者 | 说明 |
| --- | --- | --- |
| `now` | `NowArgumentProvider` | 当前本地时间，默认格式化 `yyyyMMddHHmmssfff` |
| `utcnow` | `UtcNowArgumentProvider` | 当前 UTC 时间，默认格式化 `yyyyMMddHHmmssfff` |
| `guid` | `GuidArgumentProvider` | 新生成的 GUID |
| `random` | `RandomArgumentProvider` | 1000000000 ~ int.MaxValue 的随机整数 |
| `random1` ~ `random9` | `Random1ArgumentProvider` ~ `Random9ArgumentProvider` | 指定位数的随机整数（如 `random2` 为 10~99） |
| `seconds` | `TimeSecondsArgumentProvider` | 当前 Unix 时间戳（秒） |
| `milliseconds` | `TimeMillisecondsArgumentProvider` | 当前 Unix 时间戳（毫秒） |

所有随机数提供者继承自 `BaseRandomArgumentProvider`，可据此扩展自定义范围的随机参数。

## 使用示例

参数提供者在文件存储的路径模板解析时按 dictionary key 被调用。扩展自定义参数只需实现 `ISystemArgProvider` 并标注 `[Service]` 与 `[DictionaryKey]`：

```csharp
[DictionaryKey("env")]
[Service(Lifetime = ServiceLifetime.Singleton)]
public class EnvArgumentProvider : ISystemArgProvider
{
    public string DefaultFormatter => string.Empty;

    public object GetValue() => Environment.MachineName;
}
```

随后在路径模板中即可使用 `env` 参数，与内置的 `now`、`guid` 等一致。

## 依赖

- `YS.Knife.Core`：`[Service]`、`[DictionaryKey]` 特性
- `YS.Knife.FileStorage.Core`（项目引用）：定义 `ISystemArgProvider` 接口

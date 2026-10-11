# YS.Knife.Resource.FileResourceLoader

`YS.Knife.Resource` 资源加载体系的本地文件加载器实现，负责从文件系统读取资源。

## 主要功能

- 提供 `FileResourceLoader`，实现 `IResourceLoader` 接口
- 判断 URI 是否为合法本地文件路径（排除绝对 URI 和含非法字符的路径）
- 通过 `File.OpenRead` 以只读流方式打开文件
- 优先级 `Priority = 3000`
- 通过 `[Service(Lifetime = Singleton)]` 特性自动注册到 DI 容器

## 使用示例

该加载器配合 `YS.Knife.Resource.Impl.Default` 的 `ResourceService` 使用。引用本包并完成服务注册后，即可通过 `IResourceService` 加载本地文件：

```csharp
public class MyService
{
    private readonly IResourceService resourceService;

    public MyService(IResourceService resourceService)
    {
        this.resourceService = resourceService;
    }

    public async Task<Stream> LoadFileAsync()
    {
        // 传入相对路径（或合法文件路径），由 FileResourceLoader 命中并打开
        return await resourceService.Load("templates/demo.html");
    }
}
```

## 匹配规则

`FileResourceLoader.CanLoad` 的判定逻辑：

- `templateUri` 为格式良好的绝对 URI（如 `http://...`）时，返回 `false`
- 路径中不能包含 `Path.GetInvalidPathChars()` 定义的非法字符
- 满足以上条件即认为可加载，使用 `File.OpenRead(templateUri)` 打开文件

## 依赖说明

- `YS.Knife.Resource.Core`：提供 `IResourceLoader` 抽象
- `YS.Knife.Core`：提供 `[Service]` 自动服务注册特性

通常与 `YS.Knife.Resource.Impl.Default` 一起引用，由默认资源服务按优先级调度。

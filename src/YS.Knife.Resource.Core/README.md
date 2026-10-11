# YS.Knife.Resource.Core

资源加载抽象层，定义统一的资源（模板）加载接口，供各资源加载实现（文件、程序集等）与默认资源服务使用。

## 主要功能

- `IResourceService`：面向使用方的资源加载入口接口，按资源 URI 加载并返回 `Stream`
- `IResourceLoader`：资源加载器扩展点，可实现自定义来源（文件、网络、数据库等）的资源加载
- `ResourceServiceExtensions`：提供 `LoadBytes` 扩展方法，将资源流直接读取为字节数组

## 核心接口

```csharp
public interface IResourceLoader
{
    Task<Stream> LoadTemplate(string resourceUri);
    bool CanLoad(string resourceUri);
    int Priority { get; }   // 优先级，值大者优先匹配
}

public interface IResourceService
{
    Task<Stream> Load(string resourceUri);
}
```

## 使用示例

通过依赖注入获取 `IResourceService`（需搭配一个实现包，如 `YS.Knife.Resource.Impl.Default`）：

```csharp
public class MyService
{
    private readonly IResourceService resourceService;

    public MyService(IResourceService resourceService)
    {
        this.resourceService = resourceService;
    }

    public async Task<byte[]> ReadTemplateAsync()
    {
        // LoadBytes 为扩展方法，内部调用 Load 并将流转为 byte[]
        return await resourceService.LoadBytes("templates/demo.html");
    }
}
```

## 自定义加载器

实现 `IResourceLoader` 并注册到 DI，即可为资源服务增加新的来源：

```csharp
public class MyResourceLoader : IResourceLoader
{
    public int Priority => 1000;

    public bool CanLoad(string resourceUri) => resourceUri.StartsWith("my://");

    public Task<Stream> LoadTemplate(string resourceUri)
    {
        // 自定义加载逻辑
    }
}
```

## 依赖说明

本包无外部依赖，仅包含接口与扩展方法定义。运行时需配合实现包使用：

- `YS.Knife.Resource.Impl.Default`：默认资源服务（按优先级分发到各 Loader）
- `YS.Knife.Resource.FileResourceLoader`：本地文件加载器
- `YS.Knife.Resource.AssemblyResourceLoader`：程序集内嵌资源加载器

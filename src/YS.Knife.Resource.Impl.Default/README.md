# YS.Knife.Resource.Impl.Default

`YS.Knife.Resource` 资源加载体系的默认资源服务实现，按优先级将资源 URI 分发到匹配的加载器。

## 主要功能

- 提供 `ResourceService`，实现 `IResourceService` 接口
- 注入所有已注册的 `IResourceLoader`，按 `Priority` 降序遍历
- 由第一个 `CanLoad` 返回 `true` 的加载器执行实际加载
- 无任何加载器可处理时抛出带错误码（`001`）的异常
- 通过 `[Service]` 特性自动注册到 DI 容器

## 工作原理

```csharp
public partial class ResourceService : IResourceService
{
    public Task<Stream> Load(string resourceUri)
    {
        foreach (var loader in loaders.OrderByDescending(p => p.Priority))
        {
            if (loader.CanLoad(resourceUri))
            {
                return loader.LoadTemplate(resourceUri);
            }
        }
        throw Errors.UnknowTemplateUri(resourceUri); // 错误码 001
    }
}
```

## 使用示例

注册服务（引用 `YS.Knife.Core` 的服务扫描即可自动注册），并至少引用一个加载器实现（如 `YS.Knife.Resource.FileResourceLoader`）：

```csharp
public class MyService
{
    private readonly IResourceService resourceService;

    public MyService(IResourceService resourceService)
    {
        this.resourceService = resourceService;
    }

    public async Task<byte[]> LoadAsync(string uri)
    {
        // 扩展方法 LoadBytes 定义在 YS.Knife.Resource.Core 中
        return await resourceService.LoadBytes(uri);
    }
}
```

## 依赖说明

- `YS.Knife.Resource.Core`：提供 `IResourceService` / `IResourceLoader` 抽象
- `YS.Knife.Core`：提供 `[Service]`、`[AutoConstructor]`、`[CodeExceptions]` 等源生成特性

注意：本包本身不包含任何具体加载器，需搭配 `YS.Knife.Resource.FileResourceLoader`、`YS.Knife.Resource.AssemblyResourceLoader` 等加载器包使用。

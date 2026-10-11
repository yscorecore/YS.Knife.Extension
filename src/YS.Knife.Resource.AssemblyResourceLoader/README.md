# YS.Knife.Resource.AssemblyResourceLoader

程序集嵌入资源加载器，支持以 `assembly://` URI 从程序集清单资源中读取模板流。

## 主要功能

- 实现 `YS.Knife.Resource.Core` 定义的 `IResourceLoader` 接口
- 识别 `assembly://{程序集名}/{资源名}` 格式的 URI，通过反射加载程序集并返回嵌入资源流
- 优先级为 `2000`，自动注册为单例服务，可与其他资源加载器（如文件加载器）共存

URI 格式：

```text
assembly://MyAssembly.Name/path/to/resource.txt
```

## 快速开始

1. 将资源文件标记为嵌入资源：

```xml
<ItemGroup>
  <EmbeddedResource Include="Templates\hello.txt" />
</ItemGroup>
```

2. 引用本包后，配合 `IResourceService`（来自 `YS.Knife.Resource.Core` 体系）按 URI 加载：

```csharp
var loader = new AssemblyResourceLoader();

var uri = "assembly://MyAssembly.Name/Templates.hello.txt";

if (loader.CanLoad(uri))
{
    using Stream stream = await loader.LoadTemplate(uri);
    using var reader = new StreamReader(stream);
    var content = await reader.ReadToEndAsync();
}
```

在 DI 体系中，所有 `IResourceLoader` 实现会自动被发现，`IResourceService` 按 `CanLoad` 与 `Priority` 选择合适的加载器。

## 依赖说明

- `YS.Knife.Resource.Core`：提供 `IResourceLoader` 接口定义
- `YS.Knife.Core`：提供 `[Service]` 自动注册能力
- 目标框架：net6.0

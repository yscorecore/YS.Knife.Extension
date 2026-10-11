# YS.Knife.FileStorage.CategoryProvider.Default

基于配置的文件存储分类（FileCategory）默认提供者实现。

## 主要功能

- 提供 `FileCategoryProvider`，实现 `IFileCategoryProvider` 接口，通过 `[Service]` 自动注册
- `CreateCategory(categoryName)`：从配置选项中查找指定名称的分类定义并返回；未配置时返回 `null`
- 配置项 `FileCategoriesOptions`：继承 `Dictionary<string, FileCategory>`，键为分类名，值为分类定义（如允许的扩展名、大小限制等，字段由 `YS.Knife.FileStorage.Core` 中的 `FileCategory` 定义）

## 使用示例

在配置文件中定义分类：

```json
{
  "FileCategoriesOptions": {
    "image": {
      "Extensions": [ ".png", ".jpg", ".jpeg" ],
      "MaxSize": 10485760
    },
    "document": {
      "Extensions": [ ".pdf", ".docx" ]
    }
  }
}
```

代码中通过接口使用：

```csharp
public class UploadService
{
    private readonly IFileCategoryProvider categoryProvider;

    public UploadService(IFileCategoryProvider categoryProvider)
        => this.categoryProvider = categoryProvider;

    public async Task<FileCategory> GetCategoryAsync(string categoryName)
    {
        return await categoryProvider.CreateCategory(categoryName);
    }
}
```

> 注：配置键与具体可用字段以 `YS.Knife.FileStorage.Core` 中 `FileCategory` 的定义为准。

## 依赖说明

- 项目引用 `YS.Knife.FileStorage.Core`：`IFileCategoryProvider` 接口与 `FileCategory` 模型
- `YS.Knife.Core`：`[Service]`、`[AutoConstructor]`、`[Options]` 等源生成特性

目标框架：.NET 6.0。

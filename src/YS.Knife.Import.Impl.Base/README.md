# YS.Knife.Import.Impl.Base

导入服务的通用基类，实现 `IImportService` 中"开始导入 / 结束导入 / 临时文件管理 / token 过期控制"等与具体 Excel 引擎无关的流程，供 ClosedXml、NPOI 等实现继承。

## 主要功能

### `BaseImportService`（抽象类）

- `BeginImport`：将上传的文件流保存到本地数据目录，生成 `Guid` token 并将导入信息（过期时间、文件路径）写入 `IDistributedCache`
- `EndImport`：删除临时文件并清除缓存中的导入信息
- `ReadData(token, metadata)`：校验 token 后调用抽象方法 `ReadData(filePath, metadata)`，由子类完成具体解析
- `ReadColumns`：抽象方法，由子类实现
- token 过期与不存在时抛出带错误码的异常（`001` 已过期、`002` 不存在、`004`/`005` 找不到表/列等）

### `ImportOptions` 配置

| 属性 | 说明 | 默认值 |
| --- | --- | --- |
| `DataFolder` | 临时数据文件存放目录，为空时使用系统临时目录 | `import_data` |
| `ExpiredIn` | token 过期时长（秒），范围 1 ~ 86400 | `7200` |

## 使用方式

本项目一般不直接引用，而是被具体实现包继承。自定义新的导入引擎时继承 `BaseImportService`：

```csharp
[AutoConstructor]
[Service(Lifetime = ServiceLifetime.Singleton)]
public partial class MyImportService : BaseImportService
{
    protected override Task<Dictionary<string, List<Dictionary<string, object>>>> ReadData(
        string filePath, EntityMetadata[] entityMetadata)
    {
        // 用自定义引擎解析 filePath
    }

    public override Task<Dictionary<string, List<ColumnInfo>>> ReadColumns(Guid token)
    {
        // 读取列头
    }
}
```

运行时依赖 `IDistributedCache`，请确保已注册分布式缓存（如内存缓存或 Redis）。

## 依赖

- `YS.Knife.Core`：`[Options]`、`[AutoConstructor]`、`[CodeExceptions]` 等特性
- `YS.Knife.Extensions.DistributedCache`（项目引用）：`GetObjectAsync` / `SetObjectAsync` 缓存扩展
- `YS.Knife.Import.Core`（项目引用）：抽象接口层

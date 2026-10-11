# YS.Knife.CodeMapper.Core

CodeMapper（编码映射）模块的抽象层，定义编码映射查询与管理的公共接口。

## 主要功能

- `ICodeMapperService`：编码映射查询服务，批量将源数据映射到目标编码
- `ICodeMapperManagerService<T>`：映射关系管理服务（分页查询、批量保存目标映射）
- `MapperResult<T>`：映射结果模型
- `CodeMapperServiceExtensions.MapOne`：单条数据映射扩展方法

## 快速开始

注入 `ICodeMapperService`，将一批外部数据按其源编码映射为目标编码：

```csharp
using YS.Knife.CodeMapper;

public class SyncService
{
    private readonly ICodeMapperService mapperService;

    public SyncService(ICodeMapperService mapperService)
    {
        this.mapperService = mapperService;
    }

    public async Task SyncAsync(IEnumerable<ExternalOrder> orders, CancellationToken token)
    {
        // 把外部订单号映射为内部订单号；未出现过的源编码会自动入库等待人工维护
        var results = await mapperService.MapAll(
            orders,
            group: "Order",
            sourceCodeFun: o => o.ExternalOrderNo,
            sourceNameFun: o => o.ExternalOrderName,
            autoInsertSourceData: true,
            token: token);

        foreach (var r in results)
        {
            if (r.Mapped)
            {
                Console.WriteLine($"{r.Data} -> {r.TargetCode} ({r.TargetName})");
            }
        }
    }

    // 单条映射
    public async Task<MapperResult<ExternalOrder>> SyncOneAsync(ExternalOrder order, CancellationToken token)
    {
        return await mapperService.MapOne<ExternalOrder, string, string>(
            order, "Order", o => o.ExternalOrderNo, token);
    }
}
```

## 管理接口

```csharp
public partial interface ICodeMapperManagerService<T>
{
    Task<PagedList<CodeMapperDto>> QueryPagedList(LimitQueryInfo req, CancellationToken cancellationToken = default);
    Task SaveMappers(UpdateTargetMapperReq[] req, CancellationToken cancellationToken = default);
}
```

`QueryPagedList` 用于分页查询映射关系，`SaveMappers` 用于批量维护目标编码/名称。

## 依赖说明

- 目标框架：.NET 6（`net6.0`）
- `YS.Knife.ServiceBase`：提供 `PagedList`、`LimitQueryInfo`、`BaseDto<T>` 等基础类型
- 实现层请参考 `YS.Knife.CodeMapper.Impl.EFCore`

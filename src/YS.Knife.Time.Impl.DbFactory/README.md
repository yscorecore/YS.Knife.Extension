# YS.Knife.Time.Impl.DbFactory

`YS.Knife.Time` 时间服务的数据库实现，通过执行 SQL 从数据库服务器获取当前时间，作为分布式系统的统一时间源。

## 主要功能

- 提供 `TimeService`，实现 `ITimeService` 接口
- 通过 `IDbConnectionFactory` 创建数据库连接，执行可配置的时间查询 SQL
- 查询结果包装为 `DateTimeOffset` 返回
- `TimeOptions` 支持自定义 SQL 语句，默认 `select current_timestamp`
- 通过 `[Service]` 特性自动注册到 DI 容器

## 配置

在 `appsettings.json` 中可覆盖默认 SQL（配置节名与类型名一致）：

```json
{
  "TimeOptions": {
    "NowSqlScript": "select current_timestamp"
  }
}
```

`NowSqlScript` 为必填项（`[Required]`），可按数据库类型调整，例如 SQL Server 可改为 `SELECT SYSDATETIME()`。

## 使用示例

```csharp
public class MyService
{
    private readonly ITimeService timeService;

    public MyService(ITimeService timeService)
    {
        this.timeService = timeService;
    }

    public async Task<DateTimeOffset> GetDbNowAsync(CancellationToken cancellationToken)
    {
        // 返回数据库服务器当前时间
        return await timeService.Now(cancellationToken);
    }
}
```

## 适用场景

多节点部署时，以数据库服务器时间作为统一时间基准，避免各应用节点本机时钟漂移导致的时间不一致。

## 依赖说明

- `YS.Knife.Time.Core`：提供 `ITimeService` 抽象
- `YS.Knife.Data`：提供 `IDbConnectionFactory` 数据库连接工厂
- `YS.Knife.Extensions.DbConnection`：提供 `ExecuteScalarAsync` 扩展方法
- `YS.Knife.Core`：提供 `[Service]`、`[AutoConstructor]`、`[Options]` 源生成特性

运行时需要求容器中已注册可用的 `IDbConnectionFactory`。

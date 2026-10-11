# YS.Knife.AuditLogs.Core

审计日志抽象层，定义审计日志的数据模型、访问器、写入器及数据增强器接口，不包含任何具体实现。

## 主要功能

- `IAuditLog`：审计日志模型，包含操作 Id（`OperationId`）、操作描述（`OperationDesc`）、执行时间（`ExecutionTime`）、耗时（`Duration`）、是否成功（`Success`）、异常（`Error`）及扩展数据字典（`Datas`）。
- `IAuditLogAccessor`：访问当前上下文中已创建的审计日志（类似 `HttpContextAccessor` 模式）。
- `IAuditLogWriter`：日志写入器，由具体实现负责持久化（数据库、文件、消息队列等）。
- `IAutitLogExecutingDataEnricher` / `IAutitLogExecutedDataEnricher`：在执行前 / 执行后两个阶段向日志附加数据的增强器（可获取入参 `Arguments`、返回值 `ReturnValue` 和 `ServiceProvider`）。
- `AuditLogExtensions`：`IAuditLog` 的扩展方法，便捷操作 `Datas` 字典。

## 使用示例

向当前审计日志附加数据：

```csharp
public class OrderService
{
    private readonly IAuditLogAccessor auditLogAccessor;

    public OrderService(IAuditLogAccessor auditLogAccessor)
    {
        this.auditLogAccessor = auditLogAccessor;
    }

    public void CreateOrder(...)
    {
        if (auditLogAccessor.HasCreated)
        {
            auditLogAccessor.Log.PushData("orderType", "normal");
            auditLogAccessor.Log.SetData("channel", "web");
        }
    }
}
```

扩展方法一览：

```csharp
log.PushData(key, value);        // 不存在则添加，存在则抛异常
log.TryPushData(key, value);     // 尝试添加，返回是否成功
log.SetData(key, value);         // 添加或覆盖
log.HasData(key);                // 是否存在
log.GetOrDefault<T>(key);        // 读取，不存在返回默认值
log.PopOrDefault<T>(key);        // 读取并移除
```

实现自定义写入器：

```csharp
public class DbAuditLogWriter : IAuditLogWriter
{
    public Task WriteLog(IAuditLog auditLog)
    {
        // 将 auditLog 持久化到数据库
    }
}
```

实现数据增强器：

```csharp
public class UserInfoEnricher : IAutitLogExecutedDataEnricher
{
    public Task EnrichExecutedLogData(IAuditLogContext context)
    {
        context.Log.SetData("userId", "...");
        return Task.CompletedTask;
    }
}
```

## 说明

本项目仅包含接口与扩展方法（Core 抽象层）。审计日志的拦截、上下文构建与写入调度由其他实现项目完成（命名规则：`*.Core` 为抽象，`*.Impl.Xxx` 为具体实现）。

## 依赖

无第三方包依赖，目标框架 .NET 6+。

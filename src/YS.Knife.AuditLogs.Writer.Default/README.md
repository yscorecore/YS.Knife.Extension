# YS.Knife.AuditLogs.Writer.Default

审计日志的默认写出器实现，将审计日志以格式化文本的形式写入 `ILogger` 日志管道。

## 主要功能

- `DefaultAuditLogWriter`：实现 `IAuditLogWriter` 接口，通过 `[Service]` 特性自动注册。
- 输出内容包括：操作 ID（`OperationId`）、操作描述（`OperationDesc`）、耗时（`Duration`，毫秒）、是否成功（`Success`）、执行时间（`ExecutionTime`）、异常信息（`Error`）以及扩展数据字典（`Datas`）。
- 扩展数据使用 `System.Text.Json` 序列化为 camelCase JSON 文本，并采用 `UnsafeRelaxedJsonEscaping` 以保留中文字符的可读性。

## 使用示例

```csharp
// 无需手动注册，[Service] 特性会被 YS.Knife 自动扫描注册。
// 只要容器中存在 IAuditLogWriter 的使用方（如 YS.Knife.AuditLogs.AspnetCore.Mvc），
// 审计日志即会输出到日志：

// 输出示例：
// -----------DefaultAuditLogWriter---------------
// OperationId: user.create
// OperationDesc: 创建用户
// Duration: 35 ms
// Success: True
// ExecutionTime: 2024-01-01 12:00:00.000
// Error: None
// requestIp: "127.0.0.1"
// user: "admin"
// -----------------------------------------------
```

如需将审计日志写入数据库或其他存储，可自行实现 `IAuditLogWriter` 并注册，多个 Writer 会按注册顺序依次执行。

## 依赖说明

- `YS.Knife.AuditLogs.Core`：提供 `IAuditLogWriter`、`IAuditLog` 抽象。
- `YS.Knife.Extensions`：提供 `ToJsonText` 等 JSON 扩展方法。
- `YS.Knife.Core`：提供 `[Service]`、`[AutoConstructor]` 特性。

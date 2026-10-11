# YS.Knife.Data

ADO.NET 数据库连接工厂抽象层，定义 `IDbConnectionFactory` 接口，用于解耦数据访问代码与具体数据库提供程序。

## 主要功能

- `IDbConnectionFactory`：数据库连接工厂接口，提供：
  - `CreateConnection()`：创建一个新的 `DbConnection`。
  - `Factory`：对应的 `DbProviderFactory`（可用于创建命令、参数等）。
- `IDbConnectionFactory<T>`：带泛型标记的连接工厂接口，用于在同一应用中区分多个数据库连接（以 `T` 作为命名/标记类型）。

## 使用示例

```csharp
public class ReportRepository
{
    private readonly IDbConnectionFactory connectionFactory;

    public ReportRepository(IDbConnectionFactory connectionFactory)
    {
        this.connectionFactory = connectionFactory;
    }

    public async Task<int> CountAsync()
    {
        using var conn = connectionFactory.CreateConnection();
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(1) FROM Orders";
        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
    }
}
```

多数据库场景使用泛型标记：

```csharp
// 定义标记类型
public class MainDb { }
public class LogDb { }

// 注入对应数据库的连接工厂
public class LogRepository
{
    public LogRepository(IDbConnectionFactory<LogDb> factory) { ... }
}
```

## 说明

本项目仅包含接口定义（抽象层），不包含任何具体数据库的实现。具体实现（如 SQL Server、MySQL、PostgreSQL 的连接工厂）由对应的 `*.Impl.Xxx` 项目或业务项目提供。

## 依赖

无第三方包依赖，目标框架 .NET 6+。

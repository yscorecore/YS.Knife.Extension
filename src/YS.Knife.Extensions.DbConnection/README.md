# YS.Knife.Extensions.DbConnection

`DbConnection` / `IDbConnection` 的 ADO.NET 便捷扩展方法库，简化原生 SQL 执行。

## 主要功能

- 执行标量查询：`ExecuteScalar` / `ExecuteScalarAsync` / `ExecuteScalar<T>`（支持参数字典）
- 执行非查询语句：`ExecuteNonQuery` / `ExecuteNonQueryAsync`（支持批量 SQL 数组）
- 执行 SQL 脚本：`ExecuteSqlScript` / `ExecuteSqlScriptFile`（含异步版本），按分隔行拆分脚本段，支持执行回调
- 读取数据：`ExecuteReader` / `ExecuteReaderAsTable` / `ExecuteReaderAsList`（返回字典列表、强类型对象列表或多列 `Tuple` 列表）
- 单行判断：`ExecuteExists` / `ExecuteExistsAsync`
- 单列读取：`ExecuteSingleColumnReader<T>`（按列索引或列名）
- 连接辅助：`TryOpen` / `TryOpenAsync`、`GetFactory`
- `DbProviderFactory.CreateConnection(connectionString)` 扩展

## 使用示例

```csharp
using System.Data.Common;

DbConnection connection = factory.CreateConnection("Server=...;Database=...");

// 标量查询
int count = connection.ExecuteScalar<int>(
    "select count(*) from Users where Age > @age",
    new Dictionary<string, object> { ["age"] = 18 });

// 查询为强类型列表
List<User> users = await connection.ExecuteReaderAsListAsync<User>(
    "select Id, Name from Users");

// 执行 SQL 脚本文件（按 GO 分段）
connection.ExecuteSqlScriptFile("init.sql", sqlSplit: "GO",
    callback: seg => Console.WriteLine($"执行第 {seg.StartLine} 行起，影响 {seg.Result} 行"));

// 判断是否存在
bool exists = await connection.ExecuteExistsAsync("select * from Users where Name = 'admin'");
```

所有扩展方法定义在 `System.Data` 命名空间下，引用包后直接使用。

## 依赖说明

- 项目引用 `YS.Knife.Extensions`（基础扩展方法）
- 无第三方包依赖，基于 `System.Data.Common`

目标框架：.NET 6.0。

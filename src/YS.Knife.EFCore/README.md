# YS.Knife.EFCore

基于特性（Attribute）驱动的 EF Core 扩展库，通过声明式特性替代 Fluent API 配置，同时提供通用的 CRUD 服务层实现。

## 目录

- [快速开始](#快速开始)
- [属性级特性](#属性级特性)
  - [EnumAsVarchar - 枚举存储为字符串](#enumasvarchar)
  - [JsonContent - 复杂对象以 JSON 存储](#jsoncontent)
  - [SymmetricEncryption - 字段对称加密](#symmetricencryption)
  - [ComputedColumnSql - 计算列](#computedcolumnsql)
  - [DefaultValueSql - 列默认值](#defaultvaluesql)
  - [Collation - 排序规则（属性级）](#collation属性级)
- [实体级特性](#实体级特性)
  - [CheckConstraintSql - CHECK 约束](#checkconstraintsql)
  - [ForeignKeyRefrence - 外键关系](#foreignkeyrefrence)
  - [View / NoKey - 映射数据库视图](#view--nokey)
  - [AdditionalProperty - 附加属性](#additionalproperty)
- [模型级特性](#模型级特性)
  - [ModelScopeDefaultValueSql - 全局默认值](#modelscopedefaultvaluesql)
  - [Collation - 排序规则（模型级）](#collation模型级)
- [方法级特性](#方法级特性)
  - [TableValueFunction - 表值函数](#tablevaluefunction)
- [基础设施](#基础设施)
  - [EFEntityStore - 自动注册实体仓库](#efentitystore)
  - [EFDbConnectionFactory - 数据库连接工厂](#efdbconnectionfactory)
- [查询扩展](#查询扩展)
- [CRUD 服务](#crud-服务)
- [Provider 特性过滤](#provider-特性过滤)

---

## 快速开始

在 `DbContext.OnModelCreating` 中调用 `ApplyKnifeExtensions`：

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    this.ApplyKnifeExtensions(modelBuilder);
}
```

所有标注了特性的属性将自动被扫描并应用。

---

## 属性级特性

### EnumAsVarchar

将枚举值存储为 `varchar` 字符串而非整数。

```csharp
[EnumAsVarchar(50)]
public OrderStatus Status { get; set; }

[EnumAsVarchar(varcharLength: 100, NameStyle = NameStyle.CamelCase)]
public PaymentMethod? Method { get; set; }
```

| 参数 | 说明 |
|---|---|
| `varcharLength` | 列最大长度，默认 32 |
| `NameStyle` | 命名风格（如 CamelCase），默认 Original |

支持 `Nullable<T>` 枚举。

---

### JsonContent

将复杂对象、`List<T>` 或 `Dictionary<TKey,TValue>` 以 JSON 字符串形式存储。

```csharp
[JsonContent]
public Dictionary<string, object> Metadata { get; set; }

[JsonContent]
public List<string> Tags { get; set; }

[JsonContent]
public AddressInfo Address { get; set; }
```

使用 `System.Text.Json` 进行序列化/反序列化，自动配置 `ValueComparer`。

---

### SymmetricEncryption

对 `string` 类型字段进行对称加密存储（如 AES）。

```csharp
[SymmetricEncryption("AES", "my-key")]
[MaxLength(500)]
public string IdCardNumber { get; set; }
```

**前置条件** — 在应用启动时注册密钥：

```csharp
EncryptionKeys.Set("my-key", "your-secret-passphrase");
```

| 参数 | 说明 |
|---|---|
| `algorithmName` | .NET 对称算法名称（如 `"AES"`） |
| `keyName` | 在 `EncryptionKeys` 中注册的密钥名称 |
| `Prefix` | 加密值前缀，默认 `"enc:"`，可自定义 |

**工作原理：**
- 加密时生成随机 IV，与密文拼接后 Base64 编码，加上前缀存入数据库
- 解密时检查前缀：有前缀则解密，无前缀则原样返回（兼容未加密的历史数据）
- 密钥通过 SHA-256 派生，线程安全的延迟加载

**EncryptionKeys API：**

```csharp
EncryptionKeys.Set("name", "key");              // 注册密钥
EncryptionKeys.Get("name");                      // 获取密钥（不存在则抛异常）
EncryptionKeys.TryGet("name", out var key);      // 安全获取
```

---

### ComputedColumnSql

映射计算列。

```csharp
[ComputedColumnSql("[Price] * [Quantity]")]
public decimal Total { get; set; }

[ComputedColumnSql("[Price] * [Quantity]", stored: true)]
public decimal StoredTotal { get; set; }
```

| 参数 | 说明 |
|---|---|
| `sql` | 计算表达式 |
| `stored` | 是否持久化存储（可选） |

---

### DefaultValueSql

设置列的 SQL 默认值表达式。

```csharp
[DefaultValueSql("GETDATE()")]
public DateTime CreatedAt { get; set; }

[DefaultValueSql("NEWID()")]
public Guid UniqueCode { get; set; }
```

支持 `Provider` 属性指定特定数据库提供程序的默认值（参见 [Provider 特性过滤](#provider-特性过滤)）。

---

### Collation（属性级）

设置单个列的排序规则。

```csharp
[Collation("SQL_Latin1_General_CP1_CI_AS")]
public string Name { get; set; }
```

---

## 实体级特性

### CheckConstraintSql

为实体表添加 CHECK 约束。约束名自动加上实体类名前缀。

```csharp
[CheckConstraintSql("PricePositive", "[Price] > 0")]
[CheckConstraintSql("QtyNonNegative", "[Quantity] >= 0", Provider = "Microsoft.EntityFrameworkCore.SqlServer")]
public class OrderItem
{
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}
```

---

### ForeignKeyRefrence

声明式配置外键关系，无需在 `OnModelCreating` 中使用 Fluent API。

```csharp
[ForeignKeyRefrence("CategoryId", "Category", "Products", "Id")]
public class Product
{
    public int CategoryId { get; set; }
    public Category Category { get; set; }
}
```

| 参数 | 说明 |
|---|---|
| `foreignKey` | 外键列名（支持复合外键 `string[]`） |
| `relatedMember` | 导航属性名（可为 null） |
| `collectionMember` | _principal_ 端的集合导航属性名（可为 null） |
| `principalKey` | 主键列名（支持复合主键 `string[]`） |
| `IsRequired` | 是否必需，默认 `true` |

---

### View / NoKey

将实体映射到数据库视图。

```csharp
[View("v_OrderSummary")]
[NoKey]
public class OrderSummary
{
    public string ProductName { get; set; }
    public int TotalQuantity { get; set; }
}
```

`[NoKey]` 表示无键实体，通常与视图配合使用。

---

### AdditionalProperty

向实体添加附加属性（扩展字段）。

```csharp
[AdditionalProperty(typeof(string), "TenantId")]
public class Product { ... }
```

---

## 模型级特性

标注在 `DbContext` 类上，对整个模型生效。

### ModelScopeDefaultValueSql

为所有实体中匹配的属性统一设置默认值。

```csharp
[ModelScopeDefaultValueSql("CreatedAt", typeof(DateTime), "GETDATE()")]
[ModelScopeDefaultValueSql("UpdatedAt", typeof(DateTime), "GETDATE()")]
public class AppDbContext : DbContext { ... }
```

所有实体中名为 `CreatedAt`、类型为 `DateTime` 的属性都会被设置 `GETDATE()` 默认值。

---

### Collation（模型级）

设置整个数据库的默认排序规则。

```csharp
[Collation("und-u-ks-level2")]
public class AppDbContext : DbContext { ... }
```

---

## 方法级特性

标注在 `DbContext` 的方法上。

### TableValueFunction

将 `DbContext` 方法映射为数据库表值函数（TVF）。

```csharp
public class AppDbContext : DbContext
{
    [TableValueFunction("GetTopCustomers", Schema = "dbo")]
    public IQueryable<Customer> GetTopCustomers(int minOrders)
        => FromExpression(() => GetTopCustomers(minOrders));
}
```

| 参数 | 说明 |
|---|---|
| `name` | 数据库函数名（默认使用方法名） |
| `schema` | 数据库 schema（可选） |

---

## 基础设施

### EFEntityStore

自动注册 `IEntityStore<T>` 到 DI 容器。

```csharp
[EFEntityStore]
public class AppDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }   // 自动注册 IEntityStore<Product>
    public DbSet<Order> Orders { get; set; }        // 自动注册 IEntityStore<Order>
}
```

也可以指定特定实体：

```csharp
[EFEntityStore(typeof(Product))]
public class AppDbContext : DbContext { ... }
```

---

### EFDbConnectionFactory

自动注册 `IDbConnectionFactory<T>` 到 DI 容器，用于从 `DbContext` 配置创建原始 `DbConnection`（适用于 Dapper 等微 ORM）。

```csharp
[EFDbConnectionFactory]
public class AppDbContext : DbContext { ... }
```

---

## 查询扩展

`QueryableExtensions` 提供安全的查找和去重检查方法，找不到或重复时抛出结构化异常。

```csharp
// 按 ID 查找，找不到抛出 EntityNotFound
var product = await dbContext.Products.FindOrThrowAsync(productId);

// 批量查找，保持输入顺序
var products = await dbContext.Products.FindArrayOrThrowAsync(new[] { id1, id2, id3 });

// 返回字典形式
var dict = await dbContext.Products.FindDictionaryOrThrowAsync(new[] { id1, id2 });

// 检查唯一性，重复抛出 EntityAlreadyExists
await QueryableExtensions.CheckDuplicateAsync(dbContext.Products, p => p.Name, "Widget");

// 编辑场景的去重检查（排除自身）
await QueryableExtensions.CheckEditDuplicateAsync(dbContext.Products, p => p.Name,
    new[] { (id1, "NewName1"), (id2, "NewName2") });
```

实体名称解析优先级：`[Display(Name)]` > `[DisplayName]` > `[Comment]` > 类型名。

---

## CRUD 服务

泛型 CRUD 服务实现，配合 `IEntityStore<T>` 使用。

### CreateApi

```csharp
// 自动注册为 ICreateApi<TCreateDto, TKey>
public partial class CreateApi<TEntity, TCreateDto, TKey> : ICreateApi<TCreateDto, TKey>
```

将 DTO 映射为实体，批量创建并返回 ID 数组。

### UpdateApi

```csharp
public partial class UpdateApi<TEntity, TUpdateDto, TKey> : IUpdateApi<TUpdateDto, TKey>
```

按 DTO 中的 ID 查找实体，将 DTO 属性复制到实体上。DTO 需实现 `IIdDto<TKey>`。

### DeleteApi

```csharp
public partial class DeleteApi<TEntity, TKey> : IDeleteApi<TKey>
```

按 ID 批量删除。如果实体实现了 `ISoftDeleteEntity`，则执行软删除（设置 `IsDeleted = true`）。

### QueryApi

```csharp
public partial class QueryApi<TEntity, TDto> : IQueryPageApi<TDto>
```

分页查询，自动处理：
- 软删除过滤（`ISoftDeleteEntity`）
- 排序（`ISortableEntity`）
- 创建时间倒序（`ICreationAuditedEntity`）

---

## Provider 特性过滤

继承自 `ProviderAttribute` 的特性支持 `Provider` 属性，可按数据库提供程序过滤：

```csharp
[DefaultValueSql("GETDATE()")]                                          // 始终生效
[DefaultValueSql("now()", Provider = "Npgsql.EntityFrameworkCore.PostgreSQL")]  // 仅 PostgreSQL
public DateTime CreatedAt { get; set; }
```

应用顺序：
1. 非 Provider 特性 — 始终应用
2. Provider 为空/null 的特性 — 作为回退
3. Provider 匹配当前 `Database.ProviderName` 的特性 — 特定数据库

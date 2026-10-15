# YS.Knife.EntityBase

实体基类抽象库，定义实体、软删除、可排序等常用实体接口，以及配套的 `IQueryable` 扩展方法和通用实体存储接口。

## 主要功能

- `IEntity<TKey>`：实体接口，要求具有 `Id` 主键属性。
- `ISoftDeleteEntity`：软删除实体接口（`IsDeleted` 属性），配套 `FilterDeleted()` / `IgnoreDeleted()` 查询扩展。
- `ISortableEntity`：可排序实体接口（`Order` 属性），配套 `SortByOrder()` / `SortDescByOrder()` 查询扩展。
- `IEntityStore<T>`：通用实体存储接口（查询、添加、删除、保存），配套 `AddRange` / `DeleteRange` 扩展方法。
- `IStatusFlowEntity<TStatus, TFlow>`：状态流转实体接口（`Status` 状态字段 + `Flows` 流转记录列表），配套 `AppendInitStatusFlow` / `UpdateStatusAndAppendFlow` 扩展方法与 `[FromStatus]` 流转约束特性。

## 使用示例

### 定义实体

```csharp
public class Category : IEntity<Guid>, ISoftDeleteEntity, ISortableEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public bool IsDeleted { get; set; }
    public int Order { get; set; }
}
```

### 查询扩展

`IgnoreDeleted` / `SortByOrder` 等扩展位于 `System.Linq` 命名空间，引用程序集后直接使用：

```csharp
var items = await dbContext.Categories
    .IgnoreDeleted()      // 过滤 IsDeleted == false
    .SortByOrder()        // 按 Order 升序（已排序时自动 ThenBy）
    .ToListAsync();

var topItems = dbContext.Categories
    .IgnoreDeleted()
    .SortDescByOrder();   // 按 Order 降序
```

### 状态流转

在状态枚举字段上用 `[FromStatus]` 声明允许流转到的目标状态（未标注的状态不限制流转）：

```csharp
public enum OrderStatus
{
    Created,
    [FromStatus(OrderStatus.Shipped, OrderStatus.Cancelled)]
    Paid,
    Shipped,
    Completed,
    Cancelled,
}

public class Order : IStatusFlowEntity<OrderStatus, OrderStatusFlow>
{
    public OrderStatus Status { get; set; }
    public List<OrderStatusFlow> Flows { get; set; }
}

public class OrderStatusFlow : IStatusChangeFlowEntity<OrderStatus>
{
    public int Order { get; set; }
    public OrderStatus? From { get; set; }
    public OrderStatus To { get; set; }
    public DateTimeOffset OperationTime { get; set; }
    [StringLength(64)]
    public string Operator { get; set; }
}
```

使用扩展方法初始化状态或流转状态，每次流转会自动校验合法性并追加一条流转记录：

```csharp
order.AppendInitStatusFlow("admin");                         // 以当前 Status 初始化（From 为 null，不校验）
order.UpdateStatusAndAppendFlow(OrderStatus.Paid, "admin");  // 校验并流转，非法流转抛出 InvalidOperationException
```

### 实体存储

```csharp
public class CategoryService
{
    private readonly IEntityStore<Category> store;

    public CategoryService(IEntityStore<Category> store)
    {
        this.store = store;
    }

    public async Task AddAsync(IEnumerable<Category> categories, CancellationToken ct)
    {
        store.AddRange(categories);
        await store.SaveChangesAsync(ct);
    }
}
```

`IEntityStore<T>` 接口：

```csharp
public interface IEntityStore<T>
{
    IQueryable<T> Current { get; }   // 当前可查询数据集
    void Add(T entity);
    void Delete(T entity);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

## 说明

本项目仅包含接口与扩展方法。`IEntityStore<T>` 的具体实现（如基于 EF Core 的存储）由对应的 `*.Entity.EFCore` 或 `*.Impl.Xxx` 项目提供。

## 依赖

无第三方包依赖，目标框架 .NET 6+。

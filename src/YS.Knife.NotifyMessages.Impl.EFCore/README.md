# YS.Knife.NotifyMessages.Impl.EFCore

基于 EF Core 的站内通知消息服务实现，按用户逻辑角色过滤消息主题与频道。

## 主要功能

- `NotifyMessageService`：实现 `INotifyMessageService`，自动注册到 DI（`[Service]`）
  - `QueryPagedList(LimitQueryInfo)`：按主逻辑角色（`Owner`）分页查询消息主题（`MessageTopicDto<Guid>`）
  - `ListMyChannels(LimitQueryInfo)`：查询当前用户（按其全部逻辑角色）可见的消息频道（`MessageChannelDto<Guid>`）
- 基于 `FlyTiger` 的 `[Mapper]` 特性自动生成实体到 DTO 的查询映射
- 通过 `MessageOptions` 配置参与过滤的逻辑角色提供者

## 快速开始

引用本包后服务自动注册。需要提供：

1. 一个或多个 `ILogicRoleProvider` 实现（见 `YS.Knife.LogicRoles.Core`）
2. `IEntityStore<MessageTopicEntity<Guid>>` 与 `IEntityStore<MessageChannelEntity<Guid>>`（来自 `YS.Knife.EFCore` 体系）
3. 配置 `MessageOptions`：

```csharp
services.Configure<MessageOptions>(options =>
{
    // 参与角色过滤的提供者名称
    options.LogicRoleProviders = new[] { "dept", "position" };
    // 主角色提供者
    options.MainLogicRole = "dept";
});
```

业务代码中使用：

```csharp
public class MyService
{
    private readonly NotifyMessageService notifyMessageService;

    public MyService(NotifyMessageService notifyMessageService)
    {
        this.notifyMessageService = notifyMessageService;
    }

    public async Task Demo(CancellationToken cancellationToken)
    {
        var query = new LimitQueryInfo { /* 分页过滤条件 */ };

        // 查询我（主角色）拥有的消息主题
        PagedList<MessageTopicDto<Guid>> topics =
            await notifyMessageService.QueryPagedList(query, cancellationToken);

        // 查询我可见的频道
        PagedList<MessageChannelDto<Guid>> channels =
            await notifyMessageService.ListMyChannels(query, cancellationToken);
    }
}
```

## 配置项（MessageOptions）

| 配置项 | 说明 | 必填 |
| --- | --- | --- |
| `LogicRoleProviders` | 参与频道可见性过滤的逻辑角色提供者名称数组 | 是 |
| `MainLogicRole` | 主逻辑角色提供者名称，用于主题 `Owner` 匹配 | 否 |

## 依赖说明

- `YS.Knife.NotifyMessages.Core`：提供 `INotifyMessageService` 与 DTO 定义
- `YS.Knife.NotifyMessages.Entity.EFCore`：提供消息实体定义
- `YS.Knife.EFCore`：提供 `IEntityStore<T>` 实体仓储
- `YS.Knife.LogicRoles.Core`：提供逻辑角色抽象

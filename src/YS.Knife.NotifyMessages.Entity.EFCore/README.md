# YS.Knife.NotifyMessages.Entity.EFCore

站内通知消息的 EF Core 实体定义，包含消息主题、频道、频道用户、消息等实体模型。

## 主要功能

定义了站内消息系统的核心实体（主键类型泛型化，`where T : struct`）：

- `MessageBaseEntity<T>`：消息基类，包含内容（`Content`）、内容类型（`ContentType`）、附件（`Attachments`），支持软删除（`ISoftDeleteEntity`）
- `MessageTopicEntity<T>`：消息主题，包含分类（`Category`）、标题（`Title`）、是否必须回复（`MustReply`）、所有者（`Owner`）、状态（`Status`）及关联频道
- `MessageChannelEntity<T>`：消息频道，关联主题与频道用户、消息列表，支持关闭标记（`Closed`）
- `MessageEntity<T>`：频道内的单条消息，记录发送用户（`User`）
- `MessageChannelUserEntity<T>`：频道用户，记录用户编码（`Code`）与已读时间（`ReadTime`）

所有实体继承 `YS.Knife.EntityBase` 中的 `BaseEntity<T>`。

## 使用示例

在自己的 `DbContext` 中使用 `Guid` 作为主键类型：

```csharp
public class AppDbContext : DbContext
{
    public DbSet<MessageTopicEntity<Guid>> MessageTopics => Set<MessageTopicEntity<Guid>>();
    public DbSet<MessageChannelEntity<Guid>> MessageChannels => Set<MessageChannelEntity<Guid>>();
    public DbSet<MessageChannelUserEntity<Guid>> MessageChannelUsers => Set<MessageChannelUserEntity<Guid>>();
    public DbSet<MessageEntity<Guid>> Messages => Set<MessageEntity<Guid>>();
}
```

查询某频道下的消息：

```csharp
var messages = await dbContext.Messages
    .Where(m => m.ChannelId == channelId && !m.IsDeleted)
    .OrderByDescending(m => m.CreateTime)
    .ToListAsync();
```

## 依赖说明

- `YS.Knife.EntityBase`：提供 `BaseEntity<T>`、`ISoftDeleteEntity` 等实体基础设施
- 目标框架：net6.0

本包仅包含实体定义，持久化与服务实现见 `YS.Knife.NotifyMessages.Impl.EFCore`。

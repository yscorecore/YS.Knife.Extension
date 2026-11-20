using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using YS.Knife.Entity;

namespace YS.Knife.NotifyMessages.Entity.EFCore
{
    public abstract class MessageBaseEntity<T> : BaseEntity<T>, ISoftDeleteEntity
    {
        public bool IsDeleted { get; set; }
        public string Content { get; set; }
        public string ContentType { get; set; }
        public string[] Attachments { get; set; }
    }

    public class MessageEntity<T> : MessageBaseEntity<T>
        where T : struct
    {
        public string User { get; set; }
        //消息分类
        public MessageChannelEntity<T> Channel { get; set; }
        public T ChannelId { get; set; }

    }

    public class MessageTopicEntity<T> : MessageBaseEntity<T>, ISoftDeleteEntity
         where T : struct
    {
        public string Category { get; set; }
        public string Title { get; set; }
        public bool? MustReply { get; set; }//true ,false, null

        public string Owner { get; set; }
        public int Status { get; set; }
        public List<MessageChannelEntity<T>> Channels { get; set; }
    }

    public class MessageChannelEntity<T> : BaseEntity<T>
        where T : struct
    {
        public T? TopicId { get; set; }
        public MessageTopicEntity<T> Topic { get; set; }

        public List<MessageChannelUserEntity<T>> Users { get; set; }
        public List<MessageEntity<T>> Messages { get; set; } = new List<MessageEntity<T>>();
        public string Owner { get; set; } //创建者
        public bool Closed { get; set; }
    }
    public class MessageChannelUserEntity<T> : BaseEntity<T>, ISoftDeleteEntity
         where T : struct
    {
        public MessageChannelEntity<T> Channel { get; set; }
        public T ChannelId { get; set; }
        public string Code { get; set; }
        public bool IsDeleted { get; set; }

        public DateTime ReadTime { get; set; }
    }




}

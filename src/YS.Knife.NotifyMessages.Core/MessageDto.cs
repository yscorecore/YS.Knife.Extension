using YS.Knife.Service;

namespace YS.Knife.NotifyMessages
{
    public record MessageTopicDto<T> : BaseDto<T>
    {
        public string Owner { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string ContentType { get; set; } //text/html/markdown
        public bool? MustReply { get; set; }
        public string[] Attachments { get; set; }
        public string Category { get; set; } //消息分类
        public int ChannelsCount { get; set; }
    }

    public record MessageChannelTopicDto<T> : BaseDto<T>
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string ContentType { get; set; } //text/html/markdown
        public bool? MustReply { get; set; }
        public string[] Attachments { get; set; }
        public string Category { get; set; } //消息分类
    }
    public record MessageDto<T> : BaseDto<T>
    {
        public string Owner { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string ContentType { get; set; } //text/html/markdown

        public string[] Attachments { get; set; }
    }
    public record class MessageChannelDto<T> : BaseDto<T>
        where T : struct
    {
        public T? TopicId { get; set; }
        public MessageChannelTopicDto<T> Topic { get; set; }

        // public List<MessageChannelUserEntity<T>> Users { get; set; }
        // public List<MessageChannelUserEntity<T>> Users { get; set; }
        public List<MessageDto<T>> Messages { get; set; } = new List<MessageDto<T>>();
        public string Owner { get; set; } //创建者
        public bool Closed { get; set; }
    }
}

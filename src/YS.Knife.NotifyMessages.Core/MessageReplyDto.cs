using YS.Knife.Service;

namespace YS.Knife.NotifyMessages
{

    public record MessageReplyDto<T> : BaseDto<T>
    {
        public T MessageId { get; set; }
        public string Group { get; set; }
        public string From { get; set; }
        public string Content { get; set; }
        public string ContentType { get; set; } //event 表示时间，例如阅读等
        public string[] Attachments { get; set; }
        public bool? MustReply { get; set; }
    }
}

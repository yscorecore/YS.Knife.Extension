using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YS.Knife.DataItem;
using YS.Knife.Query;

namespace YS.Knife.Message
{
    public interface IMessageService
    {
        [DataSource(nameof(MessageInfo))]
        Task<PagedList<MessageInfo>> GetMesssages(LimitQueryInfo queryInfo, CancellationToken cancellationToken);
    }
    public record MessageInfo
    {
        public string Title { get; init; }
        public string Content { get; init; }
        public string From { get; init; }
        public DateTime SendTime { get; init; }
    }
    public record MessageReplyInfo 
    {
    
    }
}

using YS.Knife.DataSource;
using YS.Knife.Query;

namespace YS.Knife.NotifyMessages
{
    public interface INotifyMessageService
    {
        Task<PagedList<MessageTopicDto<Guid>>> QueryPagedList(LimitQueryInfo req, CancellationToken cancellationToken = default);
    }
}

using YS.Knife.DataItem;

namespace YS.Knife.Time
{
    public interface ITimeService
    {
        [DataItem(nameof(Now))]
        Task<DateTimeOffset> Now(CancellationToken cancellationToken = default);
    }
}

namespace YS.Knife.Time
{
    public interface ITimeService
    {
        Task<DateTimeOffset> Now(CancellationToken cancellationToken = default);
    }
}



namespace YS.Knife.Time.Impl.Default
{
    [Service(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
    public class DefaultTimeService : ITimeService
    {
        public Task<DateTimeOffset> Now(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(DateTimeOffset.Now);
        }
    }
}

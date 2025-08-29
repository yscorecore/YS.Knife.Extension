

using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace YS.Knife.Time.Impl.Redis
{
    [Service(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
    [AutoConstructor]
    public partial class RedisTimeService : ITimeService
    {
        private readonly RedisTimeOptions redisOptions;
        public async Task<DateTimeOffset> Now(CancellationToken cancellationToken = default)
        {
            using var connection = this.CreateConnection();
            var database = connection.GetDatabase();
            var result = await database.ExecuteAsync("TIME");
            var timeparts = (long[])result;
            var redisTime = DateTimeOffset.FromUnixTimeSeconds(timeparts[0]).AddMilliseconds(timeparts[1] / 1000.0);
            return redisTime;
        }
        private ConnectionMultiplexer CreateConnection()
        {
            return redisOptions.Configuration != null ?
                    ConnectionMultiplexer.Connect(redisOptions.Configuration) :
                    ConnectionMultiplexer.Connect(redisOptions.ConnectionString);
        }
    }
}

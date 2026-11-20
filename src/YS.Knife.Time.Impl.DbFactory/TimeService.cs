using System.Data;
using YS.Knife.Data;

namespace YS.Knife.Time.Impl.EFDatabaseFacade
{
    [Service]
    [AutoConstructor]
    public partial class TimeService : ITimeService
    {
        private readonly TimeOptions timeOption;
        private readonly IDbConnectionFactory dbConnectionFactory;
        public async Task<DateTimeOffset> Now(CancellationToken cancellationToken = default)
        {
            await using var dbConnection = dbConnectionFactory.CreateConnection();
            var time = await dbConnection.ExecuteScalarAsync<DateTime>(timeOption.NowSqlScript, cancellationToken);
            return new DateTimeOffset(time);
        }
    }
}

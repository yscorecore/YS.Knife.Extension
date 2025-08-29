using StackExchange.Redis;

namespace YS.Knife.Time.Impl.Redis
{
    [Options("Redis")]
    public class RedisTimeOptions
    {
        public string ConnectionString { get; set; } = "localhost";
        public ConfigurationOptions Configuration { get; set; }
    }
}

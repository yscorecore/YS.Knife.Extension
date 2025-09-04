using StackExchange.Redis;

namespace YS.Knife.Time.Impl.Redis
{
    [Options("Redis")]
    public class RedisTimeOptions
    {
        public string Configuration { get; set; } = "localhost";
        public ConfigurationOptions ConfigurationOptions { get; set; }
    }
}

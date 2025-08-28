using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace YS.Knife.Lock.Impl.Redis
{
    public class RedisLockOptions
    {
        public string LockKeyPrefix { get; set; } = "Lock_";
        public string ConnectionString { get; set; } = "localhost";
        public ConfigurationOptions Configuration { get; set; }
    }
}

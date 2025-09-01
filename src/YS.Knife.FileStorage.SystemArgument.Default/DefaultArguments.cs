using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YS.Knife.FileStorage.SystemArgument.Default
{
    [Service(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
    public class NowArgumentProvider : ISystemArgProvider
    {
        public string Name => "now";

        public string DefaultFormatter => "yyyyMMddHHmmssfff";

        public object GetValue() => DateTime.Now;
    }
    [Service(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
    public class UtcNowArgumentProvider : ISystemArgProvider
    {
        public string Name => "utcnow";

        public string DefaultFormatter => "yyyyMMddHHmmssfff";

        public object GetValue() => DateTime.UtcNow;
    }

    [Service(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
    public class GuidArgumentProvider : ISystemArgProvider
    {
        public string Name => "guid";

        public string DefaultFormatter => string.Empty;

        public object GetValue() => Guid.NewGuid();
    }

    public class BaseRandomArgumentProvider : ISystemArgProvider
    {
        private readonly string name;
        private readonly int minValue;
        private readonly int maxValue;

        public BaseRandomArgumentProvider(string name, int minValue, int maxValue)
        {
            this.name = name;
            this.minValue = minValue;
            this.maxValue = maxValue;
        }
        public string Name => name;

        public string DefaultFormatter => string.Empty;

        public object GetValue() => Random.Shared.Next(minValue, maxValue);
    }
    [Service(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
    public class RandomArgumentProvider : BaseRandomArgumentProvider
    {
        public RandomArgumentProvider() : base("random", 1000000000, int.MaxValue)
        {
        }
    }
    [Service(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
    public class Random1ArgumentProvider : BaseRandomArgumentProvider
    {
        public Random1ArgumentProvider() : base("random1", 0, 10)
        {
        }
    }
    [Service(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
    public class Random2ArgumentProvider : BaseRandomArgumentProvider
    {
        public Random2ArgumentProvider() : base("random2", 10, 100)
        {
        }
    }
    [Service(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
    public class Random3ArgumentProvider : BaseRandomArgumentProvider
    {
        public Random3ArgumentProvider() : base("random3", 100, 1000)
        {
        }
    }
    [Service(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
    public class Random4ArgumentProvider : BaseRandomArgumentProvider
    {
        public Random4ArgumentProvider() : base("random4", 1000, 10000)
        {
        }
    }
    [Service(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
    public class Random5ArgumentProvider : BaseRandomArgumentProvider
    {
        public Random5ArgumentProvider() : base("random5", 10000, 100000)
        {
        }
    }
    [Service(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
    public class Random6ArgumentProvider : BaseRandomArgumentProvider
    {
        public Random6ArgumentProvider() : base("random6", 100000, 1000000)
        {
        }
    }
    [Service(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
    public class Random7ArgumentProvider : BaseRandomArgumentProvider
    {
        public Random7ArgumentProvider() : base("random7", 1000000, 10000000)
        {
        }
    }
    [Service(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
    public class Random8ArgumentProvider : BaseRandomArgumentProvider
    {
        public Random8ArgumentProvider() : base("random8", 10000000, 100000000)
        {
        }
    }
    [Service(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
    public class Random9ArgumentProvider : BaseRandomArgumentProvider
    {
        public Random9ArgumentProvider() : base("random9", 100000000, 1000000000)
        {
        }
    }
    [Service(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
    public class TimeSecondsArgumentProvider : ISystemArgProvider
    {
        public string Name => "timeSeconds";
        public string DefaultFormatter => string.Empty;
        public object GetValue() => DateTimeOffset.Now.ToUnixTimeSeconds();
    }
    [Service(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
    public class TimeMillisecondsArgumentProvider : ISystemArgProvider
    {
        public string Name => "timeMilliseconds";
        public string DefaultFormatter => string.Empty;
        public object GetValue() => DateTimeOffset.Now.ToUnixTimeMilliseconds();
    }
}

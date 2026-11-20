namespace YS.Knife.FileStorage.SystemArgument.Default
{
    [DictionaryKey("now")]
    [Service(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
    public class NowArgumentProvider : ISystemArgProvider
    {
        public string DefaultFormatter => "yyyyMMddHHmmssfff";

        public object GetValue() => DateTime.Now;
    }
    [DictionaryKey("utcnow")]
    [Service(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
    public class UtcNowArgumentProvider : ISystemArgProvider
    {
        public string DefaultFormatter => "yyyyMMddHHmmssfff";

        public object GetValue() => DateTime.UtcNow;
    }
    [DictionaryKey("guid")]
    [Service(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
    public class GuidArgumentProvider : ISystemArgProvider
    {
        public string DefaultFormatter => string.Empty;

        public object GetValue() => Guid.NewGuid();
    }

    public class BaseRandomArgumentProvider : ISystemArgProvider
    {
        private readonly int minValue;
        private readonly int maxValue;

        public BaseRandomArgumentProvider(int minValue, int maxValue)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
        }

        public string DefaultFormatter => string.Empty;

        public object GetValue() => Random.Shared.Next(minValue, maxValue);
    }
    [DictionaryKey("random")]
    [Service(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
    public class RandomArgumentProvider : BaseRandomArgumentProvider
    {
        public RandomArgumentProvider() : base(1000000000, int.MaxValue)
        {
        }
    }
    [DictionaryKey("random1")]
    [Service(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
    public class Random1ArgumentProvider : BaseRandomArgumentProvider
    {
        public Random1ArgumentProvider() : base(0, 10)
        {
        }
    }
    [DictionaryKey("random2")]
    [Service(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
    public class Random2ArgumentProvider : BaseRandomArgumentProvider
    {
        public Random2ArgumentProvider() : base(10, 100)
        {
        }
    }
    [DictionaryKey("random3")]

    [Service(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
    public class Random3ArgumentProvider : BaseRandomArgumentProvider
    {
        public Random3ArgumentProvider() : base(100, 1000)
        {
        }
    }
    [DictionaryKey("random4")]
    [Service(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
    public class Random4ArgumentProvider : BaseRandomArgumentProvider
    {
        public Random4ArgumentProvider() : base(1000, 10000)
        {
        }
    }
    [DictionaryKey("random5")]
    [Service(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
    public class Random5ArgumentProvider : BaseRandomArgumentProvider
    {
        public Random5ArgumentProvider() : base(10000, 100000)
        {
        }
    }
    [DictionaryKey("random6")]
    [Service(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
    public class Random6ArgumentProvider : BaseRandomArgumentProvider
    {
        public Random6ArgumentProvider() : base(100000, 1000000)
        {
        }
    }
    [DictionaryKey("random7")]
    [Service(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
    public class Random7ArgumentProvider : BaseRandomArgumentProvider
    {
        public Random7ArgumentProvider() : base(1000000, 10000000)
        {
        }
    }
    [DictionaryKey("random8")]
    [Service(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
    public class Random8ArgumentProvider : BaseRandomArgumentProvider
    {
        public Random8ArgumentProvider() : base(10000000, 100000000)
        {
        }
    }
    [DictionaryKey("random9")]
    [Service(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
    public class Random9ArgumentProvider : BaseRandomArgumentProvider
    {
        public Random9ArgumentProvider() : base(100000000, 1000000000)
        {
        }
    }
    [DictionaryKey("seconds")]
    [Service(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
    public class TimeSecondsArgumentProvider : ISystemArgProvider
    {
        public string DefaultFormatter => string.Empty;
        public object GetValue() => DateTimeOffset.Now.ToUnixTimeSeconds();
    }
    [DictionaryKey("milliseconds")]
    [Service(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
    public class TimeMillisecondsArgumentProvider : ISystemArgProvider
    {
        public string DefaultFormatter => string.Empty;
        public object GetValue() => DateTimeOffset.Now.ToUnixTimeMilliseconds();
    }
}

using YS.Knife;
using YS.Knife.DataSource;
using YS.Knife.Query;

namespace DataItemDemo
{
    public interface IService1
    {
        [DataItem("service1task")]
        Task<int> GetTaskValue();
    }
    public interface IService2
    {
        [DataItem("service2task")]
        Task<string> GetTaskValue(int v);
    }
    public interface IService3
    {
        [DataItem("service3task")]
        Task<string> GetTaskValue(int v = 128, string v2 = default, DateTime v3 = default, LimitQueryInfo limit = null);
    }
    [Service]
    public class Service1 : IService1
    {
        public Task<int> GetTaskValue()
        {
            return Task.FromResult(10);
        }
    }
    [Service]
    public class Service2 : IService2
    {
        public Task<string> GetTaskValue(int v)
        {
            return Task.FromResult("abc");
        }
    }

    [Service]
    public class Service3 : IService3
    {
        public Task<string> GetTaskValue(int v, string v2, DateTime v3, LimitQueryInfo limit)
        {
            return Task.FromResult("v3");
        }
    }
}

using YS.Knife;
using YS.Knife.DataItem;

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
        Task<string> GetTaskValue();
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
        public Task<string> GetTaskValue()
        {
            return Task.FromResult("abc");
        }
    }
}

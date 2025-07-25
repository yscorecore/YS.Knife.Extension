using System.Security.Cryptography;
using FlyTiger;
using YS.Knife;
using YS.Knife.DataSource;
using YS.Knife.Query;

namespace DataSourceDemo
{
    public interface IService1
    {
        [DataSource("service1")]
        public Task<PagedList<Service1Data>> GetDataAsync(LimitQueryInfo query);
    }
    public record Service1Data
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Value { get; set; }
    }
    [AutoConstructor]
    [Service]
    public partial class Service1 : IService1
    {
        [AutoConstructorIgnore]
        private readonly List<Service1Data> _data = new()
        {
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 1", Value = 10 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 2", Value = 20 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 3", Value = 30 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 1", Value = 10 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 2", Value = 20 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 3", Value = 30 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 1", Value = 10 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 2", Value = 20 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 3", Value = 30 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 1", Value = 10 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 2", Value = 20 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 3", Value = 30 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 1", Value = 10 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 2", Value = 20 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 3", Value = 30 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 1", Value = 10 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 2", Value = 20 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 3", Value = 30 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 1", Value = 10 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 2", Value = 20 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 3", Value = 30 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 1", Value = 10 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 2", Value = 20 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 3", Value = 30 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 1", Value = 10 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 2", Value = 20 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 3", Value = 30 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 1", Value = 10 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 2", Value = 20 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 3", Value = 30 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 1", Value = 10 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 2", Value = 20 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 3", Value = 30 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 1", Value = 10 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 2", Value = 20 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 3", Value = 30 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 1", Value = 10 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 2", Value = 20 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 3", Value = 30 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 1", Value = 10 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 2", Value = 20 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 3", Value = 30 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 1", Value = 10 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 2", Value = 20 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 3", Value = 30 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 1", Value = 10 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 2", Value = 20 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 3", Value = 30 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 1", Value = 10 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 2", Value = 20 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 3", Value = 30 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 1", Value = 10 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 2", Value = 20 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 3", Value = 30 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 1", Value = 10 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 2", Value = 20 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 3", Value = 30 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 1", Value = 10 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 2", Value = 20 },
            new Service1Data { Id = Guid.NewGuid(), Name = "Item 3", Value = 30 },
        };
        public Task<PagedList<Service1Data>> GetDataAsync(LimitQueryInfo query)
        {
            var pagedData = _data.AsQueryable().QueryPage(query);
            return Task.FromResult(pagedData);
        }
    }
}

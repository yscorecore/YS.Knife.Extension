using System.Security.Cryptography;
using FlyTiger;
using YS.Knife;
using YS.Knife.DataItem;
using YS.Knife.Query;

namespace DataSourceDemo
{
    public interface IService1
    {
        [DataSource("service1")]
        public Task<PagedList<Service1Data>> GetDataAsync(LimitQueryInfo query);

        [DataSource("service3")]
        public Task<PagedList<Service1Data>> GetData3Async(LimitQueryInfo query);

        [DataSource("service4", Arguments = new object[] { "type4" })]
        [DataSource("service5", Arguments = new object[] { "type5" })]
        public Task<PagedList<Service1Data>> GetDataByTypeAsync(string type, LimitQueryInfo query);
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

        public Task<PagedList<Service1Data>> GetData3Async(LimitQueryInfo query)
        {
            var pagedData = _data.AsQueryable().QueryPage(query);
            return Task.FromResult(pagedData);
        }

        public Task<PagedList<Service1Data>> GetDataByTypeAsync(string type, LimitQueryInfo query)
        {
            var pagedData = _data.Select(p => p with { Name = $"{type}_{p.Name}" }).AsQueryable().QueryPage(query);
            return Task.FromResult(pagedData);
        }

        public Task<PagedList<Service1Data>> GetDataAsync(LimitQueryInfo query)
        {
            var pagedData = _data.AsQueryable().QueryPage(query);
            return Task.FromResult(pagedData);
        }
    }
}

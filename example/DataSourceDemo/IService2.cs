using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using FlyTiger;
using YS.Knife;
using YS.Knife.DataSource;
using YS.Knife.Query;

namespace DataSourceDemo
{
    public interface IService2
    {
        [DataSource("service2")]
        public Task<PagedList<Service2Data>> GetDataAsync(LimitQueryInfo query);
    }
    public record Service2Data
    {
        public Guid Id { get; set; }
        [Display(Name = "名称", Order = 1)]
        public string Name { get; set; }
        [DisplayName("值")]
        public int Value { get; set; }
    }
    [AutoConstructor]
    [Service]
    public partial class Service2 : IService2
    {
        [AutoConstructorIgnore]
        private readonly List<Service2Data> _data = new()
        {
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 1", Value = 10 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 2", Value = 20 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 3", Value = 30 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 1", Value = 10 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 2", Value = 20 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 3", Value = 30 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 1", Value = 10 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 2", Value = 20 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 3", Value = 30 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 1", Value = 10 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 2", Value = 20 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 3", Value = 30 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 1", Value = 10 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 2", Value = 20 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 3", Value = 30 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 1", Value = 10 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 2", Value = 20 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 3", Value = 30 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 1", Value = 10 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 2", Value = 20 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 3", Value = 30 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 1", Value = 10 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 2", Value = 20 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 3", Value = 30 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 1", Value = 10 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 2", Value = 20 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 3", Value = 30 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 1", Value = 10 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 2", Value = 20 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 3", Value = 30 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 1", Value = 10 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 2", Value = 20 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 3", Value = 30 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 1", Value = 10 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 2", Value = 20 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 3", Value = 30 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 1", Value = 10 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 2", Value = 20 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 3", Value = 30 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 1", Value = 10 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 2", Value = 20 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 3", Value = 30 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 1", Value = 10 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 2", Value = 20 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 3", Value = 30 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 1", Value = 10 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 2", Value = 20 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 3", Value = 30 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 1", Value = 10 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 2", Value = 20 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 3", Value = 30 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 1", Value = 10 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 2", Value = 20 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 3", Value = 30 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 1", Value = 10 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 2", Value = 20 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 3", Value = 30 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 1", Value = 10 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 2", Value = 20 },
            new Service2Data { Id = Guid.NewGuid(), Name = "Item 3", Value = 30 },
        };
        public Task<PagedList<Service2Data>> GetDataAsync(LimitQueryInfo query)
        {
            var pagedData = _data.AsQueryable().QueryPage(query);
            return Task.FromResult(pagedData);
        }
    }
}

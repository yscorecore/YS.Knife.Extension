using System.Runtime.Serialization;
using Microsoft.AspNetCore.Mvc;
using YS.Knife.Query;

namespace DataSourceDemo.Controllers
{
    [ApiController]
    [Route("orders")]
    public class OrdersController : ControllerBase
    {
        private static readonly List<Order> Store = new()
        {
            new Order { Id = 1, Status = "completed", CustomerId = 100, Total = 45m,  CreatedAt = new DateTime(2026, 9, 1), ItemCount = 2 },
            new Order { Id = 2, Status = "pending",   CustomerId = 101, Total = 200m, CreatedAt = new DateTime(2026, 9, 5), ItemCount = 5 },
            new Order { Id = 3, Status = "cancelled", CustomerId = 100, Total = 30m,  CreatedAt = new DateTime(2026, 9, 8), ItemCount = 1 }
        };

        /// <summary>
        /// 列表 + 分页 + 服务端过滤/排序。
        /// 通过 datasource: GET /api/datasource/orders?Offset=0&amp;Limit=10&amp;Filter=status='pending'&amp;OrderBy=createdAt desc
        ///
        /// 通用字段见 ProductsController.GetDataAsync 的注释。
        /// </summary>
        [HttpGet]
        public Task<PagedList<Order>> GetDataAsync([FromQuery] LimitQueryInfo query, CancellationToken cancellationToken)
        {
            var paged = ProductsController.BuildPagedList(Store, query);
            return Task.FromResult(paged);
        }

        /// <summary>
        /// GET /orders/{id} —— id 是路由参数。
        /// </summary>
        [HttpGet("{id:int}")]
        public Order? GetById(int id)
        {
            return Store.FirstOrDefault(o => o.Id == id);
        }
    }

    public class Order
    {
        public int Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public decimal Total { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ItemCount { get; set; }
    }
}

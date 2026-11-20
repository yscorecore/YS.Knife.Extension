using System.Runtime.Serialization;
using Microsoft.AspNetCore.Mvc;
using YS.Knife.Query;

namespace DataSourceDemo.Controllers
{
    [ApiController]
    [Route("users")]
    public class UsersController : ControllerBase
    {
        private static readonly List<User> Store = new()
        {
            new User { Id = 1, Name = "Alice",   Role = "admin", Active = true },
            new User { Id = 2, Name = "Bob",     Role = "user",  Active = true },
            new User { Id = 3, Name = "Charlie", Role = "guest", Active = false }
        };

        /// <summary>
        /// 列表 + 分页 + 服务端过滤/排序。
        /// 通过 datasource: GET /api/datasource/users?Offset=0&amp;Limit=10&amp;Filter=role='admin'&amp;OrderBy=id asc
        ///
        /// 通用字段见 ProductsController.GetDataAsync 的注释。
        /// </summary>
        [HttpGet]
        public Task<PagedList<User>> GetDataAsync([FromQuery] LimitQueryInfo query)
        {
            var paged = ProductsController.BuildPagedList(Store, query);
            return Task.FromResult(paged);
        }

        /// <summary>
        /// GET /users/{id} —— id 是路由参数。
        /// </summary>
        [HttpGet("{id:int}")]
        public User? GetById(int id)
        {
            return Store.FirstOrDefault(u => u.Id == id);
        }
    }

    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool Active { get; set; }
    }
}

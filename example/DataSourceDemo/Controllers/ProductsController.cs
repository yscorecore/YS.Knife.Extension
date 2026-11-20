using System.Linq.Expressions;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Mvc;
using YS.Knife.Query;

namespace DataSourceDemo.Controllers
{
    [ApiController]
    [Route("products")]
    public class ProductsController : ControllerBase
    {
        private static readonly List<Product> Store = new()
        {
            new Product { Id = 1, Category = "electronics", Name = "Phone",                 Price = 699m },
            new Product { Id = 2, Category = "electronics", Name = "Laptop",                Price = 1299m },
            new Product { Id = 3, Category = "books",       Name = "ASP.NET Core in Action", Price = 45m },
            new Product { Id = 4, Category = "books",       Name = "Clean Code",            Price = 35m }
        };

        /// <summary>
        /// 列表 + 分页 + 服务端过滤/排序。
        /// 通过 datasource: GET /api/datasource/products?Offset=0&amp;Limit=10&amp;Filter=category='electronics'&amp;OrderBy=price asc
        ///
        /// LimitQueryInfo 自带字段(均为 [FromQuery] query string 绑定):
        ///   - Offset  : 跳过的记录数(默认 0)
        ///   - Limit   : 本页大小(默认 10000,[Range 0..10000])
        ///   - Filter  : 服务端过滤表达式,如 category = 'electronics' and price &gt; 100
        ///   - OrderBy : 排序,如 'price asc, id desc'
        ///   - Select  : 投影字段(可选)
        ///   - Agg     : 聚合,如 'price.sum();price.avg()'
        ///   - CountAll: 是否返回 totalCount(默认 true)
        ///   - Distinct: 去重
        /// </summary>
        [HttpGet]
        public Task<PagedList<Product>> GetDataAsync([FromQuery] LimitQueryInfo query)
        {
            var paged = BuildPagedList(Store, query);
            return Task.FromResult(paged);
        }

        /// <summary>
        /// GET /products/{id} —— id 是路由参数。
        /// 通过 datasource: 路径参数 datasource 已切断,走原生路由 /products/123。
        /// </summary>
        [HttpGet("{id:int}")]
        public Product? GetById(int id)
        {
            return Store.FirstOrDefault(p => p.Id == id);
        }

        /// <summary>
        /// POST /products —— body 参数。
        /// </summary>
        [HttpPost]
        public Product Create([FromBody] CreateProductRequest request)
        {
            var nextId = Store.Count == 0 ? 1 : Store.Max(p => p.Id) + 1;
            var product = new Product { Id = nextId, Category = request.Category, Name = request.Name, Price = request.Price };
            Store.Add(product);
            return product;
        }

        /// <summary>
        /// 通用 in-memory 分页 + 过滤 + 排序逻辑(Products/Orders/Users 共用)。
        /// 业务 controller 也可以自己实现,这里抽出来便于复用。
        /// </summary>
        internal static PagedList<T> BuildPagedList<T>(IEnumerable<T> source, LimitQueryInfo query)
            where T : class
        {
            IEnumerable<T> filtered = source;

            // 1) 服务端过滤(库自带的 Filter 字符串解析)
            if (!string.IsNullOrWhiteSpace(query.Filter))
            {
                filtered = filtered.Where(item => item.IsMatched(query.Filter!));
            }

            // 2) 服务端排序 — in-memory 简化版(逗号分隔,每段 field asc/desc)
            if (!string.IsNullOrWhiteSpace(query.OrderBy))
            {
                filtered = ApplyOrderBy(filtered, query.OrderBy!);
            }

            // 3) 分页
            //    PagedList 有两构造:接受 long totalCount(必填) 和 long? totalCount + bool hasNext
            var offset = query.Offset;
            var limit = query.Limit;
            var pageItems = filtered
                .Skip(offset)
                .Take(limit)
                .ToList();

            if (query.CountAll)
            {
                var totalCount = filtered.LongCount();
                return new PagedList<T>(pageItems, offset, limit, totalCount);
            }
            else
            {
                // 不查总数时,HasNext 退化为"本页填满就可能还有"
                var hasNext = pageItems.Count == limit;
                return new PagedList<T>(pageItems, offset, limit, (long?)null, hasNext);
            }
        }

        private static IEnumerable<T> ApplyOrderBy<T>(IEnumerable<T> source, string orderBy)
        {
            IOrderedEnumerable<T>? ordered = null;
            foreach (var rawSegment in orderBy.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                var segment = rawSegment.Trim();
                var parts = segment.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var field = parts[0];
                var desc = parts.Length > 1 &&
                           parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);

                var lambda = BuildOrderLambda<T>(field);
                ordered = (ordered, desc) switch
                {
                    (null, false) => source.OrderBy(lambda),
                    (null, true) => source.OrderByDescending(lambda),
                    (not null, false) => ordered!.ThenBy(lambda),
                    (not null, true) => ordered!.ThenByDescending(lambda),
                };
            }
            return ordered ?? source;
        }

        private static Func<T, object?> BuildOrderLambda<T>(string field)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            Expression body = parameter;
            foreach (var name in field.Split('.'))
            {
                body = Expression.PropertyOrField(body, name);
            }
            // 引用类型用 object 装箱,值类型会装箱
            var converted = Expression.Convert(body, typeof(object));
            return Expression.Lambda<Func<T, object?>>(converted, parameter).Compile();
        }
    }

    public class Product
    {
        public int Id { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    public class CreateProductRequest
    {
        public string Category { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}

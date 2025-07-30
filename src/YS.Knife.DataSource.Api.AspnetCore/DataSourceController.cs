using FlyTiger;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using YS.Knife.DataSource.Api.AspnetCore;
using YS.Knife.Query;

namespace YS.Knife.DataSource.Management
{
    [Route("api/[controller]")]
    [ApiController]
    [AutoConstructor]
    [DataSourceGenericController(typeof(DataSourceController<>))]
    public partial class DataSourceController<T> : ControllerBase
    {
        private readonly IDataSourceService dataSourceService;

        [HttpGet]
        [Route("load-data")]
        public Task<PagedList<T>> LoadData([FromRoute] string datasourceName, [FromQuery] LimitQueryInfo queryInfo, CancellationToken cancellationToken)
        {
            return dataSourceService.LoadData<T>(datasourceName, queryInfo, cancellationToken);
        }
        [HttpGet]
        [Route("agg")]
        public Task<object> Agg([FromRoute] string datasourceName, [FromQuery] LimitQueryInfo queryInfo, CancellationToken cancellationToken)
        {
            return dataSourceService.Agg(datasourceName, queryInfo, cancellationToken);
        }

        [HttpGet]
        [Route("list-all")]
        public Task<List<T>> All([FromRoute] string datasourceName, [FromQuery] QueryInfo queryInfo, CancellationToken cancellationToken)
        {
            return dataSourceService.All<T>(datasourceName, queryInfo, cancellationToken);
        }
        [HttpGet]
        [Route("count")]
        public Task<long> Count([FromRoute] string datasourceName, [FromQuery] string filter, CancellationToken cancellationToken)
        {
            return dataSourceService.Count(datasourceName, filter, cancellationToken);
        }

        [HttpGet]
        [Route("find-by-{key}/{value}")]
        public Task<T> FindBy([FromRoute] string datasourceName, [FromRoute] string key, [FromRoute] string value, CancellationToken cancellationToken)
        {
            return dataSourceService.FindBy<T>(datasourceName, key, value, cancellationToken);
        }
    }
}

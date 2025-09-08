using Microsoft.AspNetCore.Mvc;
using YS.Knife.Query;

namespace YS.Knife.DataItem.Api.AspnetCore
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
        public Task<PagedList<T>> LoadData([FromQuery] LimitQueryInfo queryInfo, CancellationToken cancellationToken)
        {
            var datasourceName = GetDatasourceNameFromContext();
            return dataSourceService.LoadData<T>(datasourceName, queryInfo, cancellationToken);
        }



        [HttpGet]
        [Route("agg")]
        public Task<object> Agg([FromQuery] LimitQueryInfo queryInfo, CancellationToken cancellationToken)
        {
            var datasourceName = GetDatasourceNameFromContext();
            return dataSourceService.Agg(datasourceName, queryInfo, cancellationToken);
        }

        [HttpGet]
        [Route("list-all")]
        public Task<List<T>> All([FromQuery] QueryInfo queryInfo, CancellationToken cancellationToken)
        {
            var datasourceName = GetDatasourceNameFromContext();
            return dataSourceService.All<T>(datasourceName, queryInfo, cancellationToken);
        }
        [HttpGet]
        [Route("count")]
        public Task<long> Count([FromQuery] string filter, CancellationToken cancellationToken)
        {
            var datasourceName = GetDatasourceNameFromContext();
            return dataSourceService.Count(datasourceName, filter, cancellationToken);
        }

        [HttpGet]
        [Route("find-by-{key}/{value}")]
        public Task<T> FindBy([FromRoute] string key, [FromRoute] string value, CancellationToken cancellationToken)
        {
            var datasourceName = GetDatasourceNameFromContext();
            return dataSourceService.FindBy<T>(datasourceName, key, value, cancellationToken);
        }
        private string GetDatasourceNameFromContext()
        {
            return this.HttpContext.GetEndpoint()?.Metadata?.GetMetadata<DataSourceNameAttribute>()?.Name;
        }
    }
}

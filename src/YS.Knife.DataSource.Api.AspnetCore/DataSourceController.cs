using FlyTiger;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using YS.Knife.Query;

namespace YS.Knife.DataSource.Management
{
    [Route("api/[controller]")]
    [ApiController]
    [AutoConstructor]
    public partial class DataSourceController : ControllerBase
    {
        private readonly IDataSourceService dataSourceService;

        [HttpGet]
        [Route("list-names")]
        public Task<List<string>> AllSources()
        {
            return dataSourceService.AllSources();
        }

        [HttpGet]
        [Route("{name}/load-data")]
        public Task<IPagedList> LoadData([FromRoute] string name, [FromQuery] LimitQueryInfo queryInfo)
        {
            return dataSourceService.LoadData(name, queryInfo);
        }
        [HttpGet]
        [Route("{name}/agg")]
        public Task<object> Agg([FromRoute] string name, [FromQuery] LimitQueryInfo queryInfo)
        {
            return dataSourceService.Agg(name, queryInfo);
        }

        [HttpGet]
        [Route("{name}/list-all")]
        public Task<object> All([FromRoute] string name, [FromQuery] QueryInfo queryInfo)
        {
            return dataSourceService.All(name, queryInfo);
        }
        [HttpGet]
        [Route("{name}/count")]
        public Task<long> Count([FromRoute] string name, [FromQuery] string filter)
        {
            return dataSourceService.Count(name, filter);
        }

        [HttpGet]
        [Route("{name}/find-by-{key}/{value}")]
        public Task<object> FindBy([FromRoute] string name, [FromRoute] string key, [FromRoute] string value)
        {
            return dataSourceService.FindBy(name, key, value);
        }
    }
}

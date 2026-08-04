using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using YS.Knife.DataItem.Api.AspnetCore.Internal;

namespace YS.Knife.DataItem.Api.AspnetCore
{
    [ApiController]
    [Route("api/[controller]")]
    [AutoConstructor]
    [FlyTiger.Logger]
    public partial class DataItemsController : ControllerBase
    {
        private readonly IDataItemWebService dataItemWebService;


        [HttpGet]
        public async Task<Dictionary<string, object>> LoadData([FromQuery(Name = "di")] string[] dataItems, CancellationToken cancellationToken)
        {
            return await dataItemWebService.LoadData(dataItems, cancellationToken);
        }
     

        [HttpGet]
        [Route("all")]
        public Task<List<DataItemDesc>> ListItems()
        {
            return dataItemWebService.ListItems();
        }
    }

}

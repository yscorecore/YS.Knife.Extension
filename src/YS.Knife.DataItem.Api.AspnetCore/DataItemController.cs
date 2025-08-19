using Microsoft.AspNetCore.Mvc;

namespace YS.Knife.DataItem.Api.AspnetCore
{
    [ApiController]
    [Route("api/[controller]")]
    [AutoConstructor]
    public partial class DataItemController<T, TArg> : ControllerBase
    {
        private readonly IDataItemService dataItemService;

        [HttpGet]
        public Task<T> LoadItemData([FromQuery] TArg queryInfo, CancellationToken cancellationToken)
        {
            var dataItemName = GetDatasourceNameFromContext();
            return dataItemService.GetItem<T, TArg>(dataItemName, queryInfo, cancellationToken);
        }
        private string GetDatasourceNameFromContext()
        {
            return this.HttpContext.GetEndpoint()?.Metadata?.GetMetadata<DataItemNameAttribute>()?.Name;
        }
    }
}

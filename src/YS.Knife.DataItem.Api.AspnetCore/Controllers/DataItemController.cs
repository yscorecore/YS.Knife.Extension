using Microsoft.AspNetCore.Mvc;
using YS.Knife.DataItem.Api.AspnetCore.Internal;

namespace YS.Knife.DataSource.Api.AspnetCore
{
    [ApiController]
    [Route("api/[controller]")]
    [AutoConstructor]
    [DataItemGenericController(typeof(DataItemController<,,,,,,,,,>))]
    public partial class DataItemController<TResult, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TName> : ControllerBase
    {
        private readonly IDataItemService dataItemService;

        [HttpGet]
        public async Task<TResult> LoadItemData(
            [FromQuery] TArg1 arg1,
            [FromQuery] TArg2 arg2,
            [FromQuery] TArg3 arg3,
            [FromQuery] TArg4 arg4,
            [FromQuery] TArg5 arg5,
            [FromQuery] TArg6 arg6,
            [FromQuery] TArg7 arg7,
            [FromQuery] TArg8 arg8,
            CancellationToken cancellationToken)
        {
            var dataItemName = GetDataItemNameFromContext();
            var argTypes = new Type[] { typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8) };
            var args = new object[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8 };
            var actualArgCount = argTypes.TakeWhile(x => x != typeof(NullObject)).Count();
            var res = await dataItemService.GetItem(dataItemName, args.Take(actualArgCount).ToArray(), cancellationToken);
            return (TResult)res;
        }

        private string GetDataItemNameFromContext()
        {
            return this.HttpContext.GetEndpoint()?.Metadata?.GetMetadata<DataItemNameAttribute>()?.Name;
        }
    }
}

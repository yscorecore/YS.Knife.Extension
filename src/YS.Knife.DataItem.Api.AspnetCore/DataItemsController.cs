using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace YS.Knife.DataItem.Api.AspnetCore
{
    [ApiController]
    [Route("api/[controller]")]
    [AutoConstructor]
    public partial class DataItemsController : ControllerBase
    {
        private const string DataItemName = "di";
        private readonly IDataItemService dataItemService;

        [HttpGet]
        public Task<Dictionary<string, object>> LoadItemsData([FromQuery(Name = "di")] string[] dataItems, CancellationToken cancellationToken)
        {
            var arguments = CreateDateItemArgs();
            return dataItemService.GetItems(dataItems, arguments, cancellationToken);
        }
        private DataItemArguments CreateDateItemArgs()
        {
            var commonArgs = new Dictionary<string, StringValues>();
            var namedArgs = new Dictionary<string, Dictionary<string, StringValues>>();
            foreach (var item in this.Request.Query.Where(p => p.Key != DataItemName))
            {
                var dotIndex = item.Key.IndexOf('.');
                if (dotIndex >= 1)
                {
                    var name = item.Key[..dotIndex];
                    var arg = item.Key[(dotIndex + 1)..];
                    if (namedArgs.TryGetValue(name, out var list))
                    {
                        list[arg] = item.Value;
                    }
                    else
                    {
                        namedArgs.Add(name, new Dictionary<string, StringValues>
                        {
                            [arg] = item.Value
                        });
                    }
                }
                else
                {
                    commonArgs[item.Key] = item.Value;
                }

            }

            return new DataItemArguments
            {
                CommonArguments = commonArgs,
                ItemArguments = namedArgs
            };
        }
    }

}

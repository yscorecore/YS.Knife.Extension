using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace YS.Knife.KeyValue.Api.AspnetCore
{
    [ApiController]
    [Route("api/[controller]")]
    [AutoConstructor]
    [KeyValueGroupGenericController(typeof(KeyValueController<>))]
    public partial class KeyValueController<T> : ControllerBase
        where T : KeyValueGroup
    {
        private readonly T group;
        private readonly IKeyValueService keyValueService;
        private readonly IOptions<JsonOptions> options;

        [HttpDelete]
        [Route("{key}")]
        public async Task Delete([FromRoute] string key, CancellationToken cancellationToken)
        {
            await keyValueService.Delete(group, key, cancellationToken);
        }
        [HttpGet]
        [Route("{key}")]
        public async Task<object> GetValue([FromRoute] string key, CancellationToken cancellationToken)
        {
            return await keyValueService.GetValue<object>(group, key, default,
                 options.Value.JsonSerializerOptions, cancellationToken);
        }
        [HttpPost]
        [Route("{key}")]
        public async Task SetValue([FromRoute] string key, [FromBody] object value, CancellationToken cancellationToken)
        {
            await keyValueService.SetValue(group,
                key, value,
                options.Value.JsonSerializerOptions, cancellationToken);
        }
    }
}

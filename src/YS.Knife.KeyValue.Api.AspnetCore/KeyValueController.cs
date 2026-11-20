using Microsoft.AspNetCore.Mvc;

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

        [HttpDelete]
        [Route("{key}")]
        public async Task Delete([FromRoute] string key, CancellationToken cancellationToken)
        {
            await keyValueService.Delete(group, key, cancellationToken);
        }
        [HttpGet]
        [Route("{key}")]
        public Task<object> GetValue([FromRoute] string key, CancellationToken cancellationToken = default)
        {
            return keyValueService.GetValue<object>(group, key, cancellationToken);
        }
        [HttpPost]
        public Task SetValue([FromBody] KeyValuePair<string, object> body, [FromQuery] bool keepstring = true, CancellationToken cancellationToken = default)
        {
            return keyValueService.SetValue(group, body.Key, body.Value, keepstring, cancellationToken);
        }
    }
}

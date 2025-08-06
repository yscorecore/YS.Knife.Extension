using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using YS.Knife.AspnetCore.Mvc;

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

    public class KeyValueGroupGenericControllerAttribute : GenericControllerAttribute
    {
        public KeyValueGroupGenericControllerAttribute(Type genericControllerType) : base(genericControllerType)
        {
        }

        protected override IEnumerable<Type[]> GetAllGenericTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly =>
                {
                    return !assembly.FullName.StartsWith("System", StringComparison.OrdinalIgnoreCase)
                            && !assembly.FullName.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase);
                })
                  .SelectMany(assembly => assembly.GetTypes())
                  .Where(type => type.IsSubclassOf(typeof(KeyValueGroup)) && !type.IsAbstract && type.GetCustomAttribute<ServiceAttribute>() != null)
                  .Select(type => new[] { type });
        }

        protected override string ResolveControllerName(Type[] genericTypes)
        {
            return genericTypes[0].Name;
        }
    }
}

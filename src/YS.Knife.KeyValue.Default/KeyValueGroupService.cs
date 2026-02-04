
namespace YS.Knife.KeyValue
{
    [AutoConstructor]
    [Service]
    public partial class KeyValueGroupService<TGroup> :IKeyValueService<TGroup>
        where TGroup : KeyValueGroup
    {
        private readonly TGroup group;
        private readonly IKeyValueService keyValueService;
        public Task Delete(string key, CancellationToken cancellationToken = default)
        {
            return keyValueService.Delete(group, key, cancellationToken);
        }

        public Task<T> GetValue<T>(string key, CancellationToken cancellationToken = default)
        {
            return keyValueService.GetValue<T>(group, key, cancellationToken);
        }
        public Task<object> Get(string key, CancellationToken cancellationToken)
        {
            return this.GetValue<object>(key, cancellationToken);
        }
        public Task SetValue(string key, object value, bool keepString, CancellationToken cancellationToken = default)
        {
            return keyValueService.SetValue(group, key, keepString, cancellationToken);
        }
    }


}

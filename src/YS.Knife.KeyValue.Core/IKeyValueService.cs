namespace YS.Knife.KeyValue
{
    public interface IKeyValueService
    {
        public Task<object> Get(string key, CancellationToken cancellationToken)
        {
            return this.GetValue<object>(key, cancellationToken);
        }
        Task<T> GetValue<T>(string key, CancellationToken cancellationToken = default);
        Task SetValue(string key, object value, bool keepString, CancellationToken cancellationToken = default);
        Task Delete(string key, CancellationToken cancellationToken = default);
    }


}

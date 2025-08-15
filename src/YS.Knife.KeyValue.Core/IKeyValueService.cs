namespace YS.Knife.KeyValue
{
    public interface IKeyValueService
    {
        Task<T> GetValue<T>(string key, CancellationToken cancellationToken = default);
        Task SetValue(string key, object value, bool keepString, CancellationToken cancellationToken = default);
        Task Delete(string key, CancellationToken cancellationToken = default);
    }


}

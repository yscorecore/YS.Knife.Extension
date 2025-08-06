namespace YS.Knife.KeyValue
{
    public interface IKeyValueService
    {
        Task<string> GetValue(string key, CancellationToken cancellationToken = default);
        Task SetValue(string key, string value, CancellationToken cancellationToken = default);
        Task Delete(string key, CancellationToken cancellationToken = default);
    }


}

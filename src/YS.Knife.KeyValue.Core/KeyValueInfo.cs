namespace YS.Knife.KeyValue
{
    public interface IKeyValueService
    {
        Task<string> GetValue(string key, CancellationToken cancellationToken = default);
    }
}

namespace YS.Knife.Metadata
{
    public interface IMetadataService
    {
        Task<List<string>> ListAll(CancellationToken cancellationToken = default);

        Task<MetadataInfo> GetMetadataInfo(string name, CancellationToken cancellationToken = default);
    }
}

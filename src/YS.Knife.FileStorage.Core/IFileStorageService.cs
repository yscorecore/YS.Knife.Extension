namespace YS.Knife.FileStorage
{
    public interface IFileStorageService
    {
        Task<FileObject> PutObject(string key, Stream content, IDictionary<string, object> metadata, CancellationToken cancellationToken = default);

        Task<FileObject> MoveObject(string key, string newKey);

        Task<FileObject> GetObject(string key);
    }

}

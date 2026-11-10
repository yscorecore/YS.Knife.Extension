
namespace YS.Knife.FileStorage
{
    public interface IFileStorageService
    {
        Task<FileObject> PutObject(string key, Stream content, IDictionary<string, object> metadata, CancellationToken cancellationToken = default);

        Task<FileObject> MoveObject(string key, string newKey, CancellationToken cancellationToken = default);

        Task<FileObject> GetObject(string key);

        Task<bool> Exists(string key, CancellationToken cancellationToken = default);

        Task<ClientUploadInfo> GetClientUploadInfo(string key, long fileSize, IDictionary<string, object> metadata, CancellationToken cancellationToken = default);
    }

}

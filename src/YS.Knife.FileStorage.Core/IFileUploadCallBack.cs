
namespace YS.Knife.FileStorage
{
    public interface IFileUploadCallBack
    {
        Task<FileObject> OnFileUploaded(FileObject file, IDictionary<string, object> metadata, CancellationToken cancellationToken = default);
    }
}

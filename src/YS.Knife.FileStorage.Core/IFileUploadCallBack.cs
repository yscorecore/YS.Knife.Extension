
namespace YS.Knife.FileStorage
{
    public interface IFileUploadCallBack
    {
        Task OnFileUploaded(FileObject file, IDictionary<string, string> userArgs, CancellationToken cancellationToken = default);
    }
}

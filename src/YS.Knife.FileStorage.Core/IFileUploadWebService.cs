
namespace YS.Knife.FileStorage
{
    public interface IFileUploadWebService
    {
        Task<FileUploadObject> Upload(string name, CancellationToken cancellationToken = default);
    }
}

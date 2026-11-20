
using System.ComponentModel.DataAnnotations;

namespace YS.Knife.FileStorage
{
    public interface IFileUploadWebService
    {
        Task<FileUploadObject> Upload(string name, CancellationToken cancellationToken = default);

        Task<FileUploadRequestInfo> GetUploadInfo(string name, CancellationToken cancellationToken);

        Task<ClientUploadInfo> GetClientUploadInfo(string name, string fileName, long size, CancellationToken cancellationToken = default);
        public record FileUploadRequestInfo
        {
            public string ServiceName { get; set; }
            public string FileFormName { get; set; } = "file";
            public long MaxLength { get; set; } = 4 * 1024 * 1024;
            public string[] AllowExtensions { get; set; }
            public UploadMode Mode { get; set; }
        }
    }
}

namespace YS.Knife.FileStorage
{
    public interface IFileUploadService
    {
        Task<FileUploadObject> Upload(string category, Stream stream, string streamFileName, long streamLength, string streamContentType, IDictionary<string, string> userArgs, CancellationToken cancellationToken = default);
    }

    public static class FileUploadServiceExtensions
    {
        public static async Task<FileObject> Upload(this IFileUploadService service, string category, Stream stream, string streamFileName, long streamLength, IDictionary<string, string> userArgs = null, CancellationToken cancellationToken = default)
        {
            string mimeType = ContentTypeMappings.GetContentTypeByFileName(streamFileName);
            return await service.Upload(category, stream, streamFileName, streamLength, mimeType, userArgs, cancellationToken);
        }

        public static async Task<FileObject> UploadBytes(this IFileUploadService service, string category, byte[] content, string fileName, IDictionary<string, string> userargs = null, CancellationToken token = default)
        {
            _ = content ?? throw new ArgumentNullException(nameof(content));
            string mimeType = ContentTypeMappings.GetContentTypeByFileName(fileName);
            await using var stream = new MemoryStream(content);
            return await service.Upload(category, stream, fileName, content.Length, mimeType, userargs, token);
        }

        public static async Task<FileObject> UploadBase64(this IFileUploadService service, string category, string base64, string fileName, IDictionary<string, string> userargs = null, CancellationToken token = default)
        {
            _ = base64 ?? throw new ArgumentNullException(nameof(base64));
            var content = Convert.FromBase64String(base64);
            return await service.UploadBytes(category, content, fileName, userargs, token);
        }

        public static async Task<FileObject> UploadFile(this IFileUploadService service, string category, string filePath, IDictionary<string, string> userargs = null, CancellationToken token = default)
        {
            string fileName = Path.GetFileName(filePath);
            string mimeType = ContentTypeMappings.GetContentTypeByFileName(fileName);
            await using var stream = File.OpenRead(filePath);
            return await service.Upload(category, stream, fileName, stream.Length, mimeType, userargs, token);
        }
    }
}

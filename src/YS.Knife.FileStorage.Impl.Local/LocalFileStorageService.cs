using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YS.Knife.FileStorage.Impl.Local
{
    [AutoConstructor]
    [Service]
    public partial class LocalFileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment hostingEnvironment;
        private readonly UploadOptions uploadOptions;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public Task<FileObject> GetObject(string key)
        {
            var fileObj = new FileObject
            {
                Key = key,
                PublicUrl = GetAccessUrl(key)
            };
            return Task.FromResult(fileObj);
        }

        public Task<FileObject> MoveObject(string key, string newKey, CancellationToken cancellationToken = default)
        {
            var oldPath = GetFileFullPath(key);
            var newPath = GetFileFullPath(newKey);
            CreateFolderIfNotExists(newPath);
            File.Move(oldPath, newPath);
            var fileObj = new FileObject
            {
                Key = newKey,
                PublicUrl = GetAccessUrl(newKey)
            };
            return Task.FromResult(fileObj);
        }

        public async Task<FileObject> PutObject(string key, Stream content, IDictionary<string, object> metadata, CancellationToken cancellationToken = default)
        {
            var filePath = GetFileFullPath(key);
            CreateFolderIfNotExists(filePath);
            await using var stream = File.OpenWrite(filePath);
            await content.CopyToAsync(stream, cancellationToken);
            return new FileObject
            {
                Key = key,
                PublicUrl = GetAccessUrl(key)
            };
        }
        private string GetFileFullPath(string key)
        {
            var wwwRootPath = hostingEnvironment.WebRootPath ?? throw new Exception("WebRootPath path not exists");
            return Path.Combine(wwwRootPath, uploadOptions.UploadRootFolder, key.Replace('/', Path.DirectorySeparatorChar));
        }
        private void CreateFolderIfNotExists(string fileFullPath)
        {
            var folder = Path.GetDirectoryName(fileFullPath);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
        }
        private string GetAccessUrl(string key)
        {
            var request = _httpContextAccessor.HttpContext.Request;
            return $"{request.Scheme}://{request.Host}{request.PathBase}/{uploadOptions.UploadRootFolder}/{key}".Replace('\\', '/');
        }
    }
}

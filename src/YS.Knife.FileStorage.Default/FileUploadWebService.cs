using Microsoft.AspNetCore.Http;

namespace YS.Knife.FileStorage.Default
{
    [AutoConstructor]
    [Service]
    public partial class FileUploadWebService : IFileUploadWebService
    {
        private readonly FileUploadService fileUploadService;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IFileCategoryProvider fileCategoryProvider;

        public async Task<ClientUploadInfo> GetClientUploadInfo(string name, CancellationToken cancellationToken = default)
        {
            var category = await fileCategoryProvider.CreateCategory(name);
            if (category.Mode != UploadMode.Client)
            {
                throw new Exception($"The file category '{name}' does not support client upload");
            }
            var request = httpContextAccessor.HttpContext.Request;
            var userArgs = request.Query.ToDictionary(k => k.Key, v => v.Value.ToString());
            if (request.HasFormContentType)
            {
                var userFormArgs = request.Form.ToDictionary(p => p.Key, v => v.Value.ToString());
                userArgs.Merge(userFormArgs);
            }
            return await fileUploadService.GetClientUploadInfo(name, userArgs, cancellationToken);
        }

        public async Task<IFileUploadWebService.FileUploadRequestInfo> GetUploadInfo(string name, CancellationToken cancellationToken)
        {
            var category = await fileCategoryProvider.CreateCategory(name);
            return new IFileUploadWebService.FileUploadRequestInfo()
            {
                AllowExtensions = category.AllowExtensions,
                FileFormName = category.FileFormName,
                MaxLength = category.MaxLength,
                ServiceName = category.ServiceName,
                Mode = category.Mode
            };
        }

        public async Task<FileUploadObject> Upload(string category, CancellationToken cancellationToken = default)
        {
            var request = httpContextAccessor.HttpContext.Request;
            return await Upload(category, request, cancellationToken);
        }
        private async Task<FileUploadObject> Upload(string category, HttpRequest request, CancellationToken cancellationToken = default)
        {
            var categoryObj = await fileCategoryProvider.CreateCategory(category) ?? throw new Exception($"The file category '{category}' is not defined");
            if (categoryObj.Mode != UploadMode.Server)
            {
                throw new Exception($"The file category '{category}' does not support server upload");
            }
            var formFile = request.Form.Files[categoryObj.FileFormName] ?? throw new Exception($"Missing form file field '{categoryObj.FileFormName}'");
            var userArgs = request.Query.ToDictionary(k => k.Key, v => v.Value.ToString());
            var userFormArgs = request.Form.ToDictionary(p => p.Key, v => v.Value.ToString());
            userArgs.Merge(userFormArgs);
            return await fileUploadService.Upload(category, categoryObj, formFile.OpenReadStream(), formFile.FileName, formFile.Length, formFile.ContentType, userArgs, cancellationToken);
        }
    }
}

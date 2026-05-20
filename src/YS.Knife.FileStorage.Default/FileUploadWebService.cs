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

        public async Task<FileUploadObject> Upload(string category, CancellationToken cancellationToken = default)
        {
            var request = httpContextAccessor.HttpContext.Request;
            return await Upload(category, request, cancellationToken);
        }
        private async Task<FileUploadObject> Upload(string category, HttpRequest request, CancellationToken cancellationToken = default)
        {
            var categoryObj = await fileCategoryProvider.CreateCategory(category) ?? throw new Exception($"The file category '{category}' is not defined");
            var formFile = request.Form.Files[categoryObj.FileFormName] ?? throw new Exception($"Missing form file field '{categoryObj.FileFormName}'");
            var userArgs = request.Query.ToDictionary(k => k.Key, v => v.Value.ToString());
            return await fileUploadService.Upload(category, categoryObj, formFile.OpenReadStream(), formFile.FileName, formFile.Length, formFile.ContentType, userArgs, cancellationToken);
        }
    }
}

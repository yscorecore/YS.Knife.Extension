using Microsoft.AspNetCore.Mvc;
using YS.Knife.FileStorage;

namespace YS.Knife.DataSource.Management
{
    [Route("api/[controller]")]
    [ApiController]
    [AutoConstructor]
    public partial class FileController : ControllerBase
    {
        private readonly IFileCategoryProvider fileCategoryFactory;
        private readonly IFileStorageService fileStorageService;
        private readonly IEnumerable<IFileStreamInterceptor> streamInterceptors;
        private readonly IEnumerable<ISystemArgProvider> systemArgsProvider;

        [HttpPost]
        [Route("upload/{category}")]
        public async Task<FileObject> Upload([FromRoute] string category)
        {
            var categoryObj = await fileCategoryFactory.CreateCategory(category) ?? throw new Exception($"The file category '{category}' is not defined");
            var formFile = this.Request.Form.Files[categoryObj.FileFormName] ?? throw new Exception($"Missing form file field '{categoryObj.FileFormName}'");
            //元数据
            var inputArgs = this.Request.Query.ToDictionary(k => k.Key, v => v.Value.ToString());
            var formFileArgs = new ISystemArgProvider[] { new FileNameArg(formFile), new FileExtArg(formFile) };
            var systemArgs = systemArgsProvider.Concat(formFileArgs).ToDictionary(p => p.Name, StringComparer.InvariantCultureIgnoreCase);
            var meta = new Dictionary<string, object>();
            foreach (var item in categoryObj.Metadata ?? new Dictionary<string, object>())
            {
                if (item.Value is string strValue)
                {
                    meta[item.Key] = FillTemplate(strValue, inputArgs, systemArgs);
                }
                else
                {
                    meta[item.Key] = item.Value;
                }
            }
            //文件名
            var fileName = FillTemplate(categoryObj.PathTemplate, inputArgs, systemArgs);
            //处理流
            var stream = formFile.OpenReadStream();
            foreach (var interceptor in categoryObj.Interceptors ?? Array.Empty<string>())
            {
                stream = RunInterceptor(stream, interceptor);
                stream.Position = 0;
            }
            return await fileStorageService.PutObject(fileName, stream, categoryObj.Metadata);

        }
        private Stream RunInterceptor(Stream stream, string interceptorName)
        {
            var interceptor = streamInterceptors.FirstOrDefault(p => p.Name.Equals(interceptorName, StringComparison.InvariantCultureIgnoreCase));
            if (interceptor != null)
            {
                return interceptor.HandlerStream(stream, this.HttpContext.RequestAborted);
            }
            else
            {
                throw new Exception($"The file stream interceptor '{interceptorName}' is not defined");
            }
        }
        private string FillTemplate(string template, IDictionary<string, string> userArgs, IDictionary<string, ISystemArgProvider> systemArgs)
        {
            return TemplatePlaceholder.Instance.FillPlaceholder(template, userArgs, systemArgs);
        }
        class FileNameArg : ISystemArgProvider
        {
            private readonly IFormFile formFile;

            public FileNameArg(IFormFile formFile)
            {
                this.formFile = formFile;
            }
            public string Name => "name";

            public string DefaultFormatter => string.Empty;

            public object GetValue()
            {
                return Path.GetFileNameWithoutExtension(formFile.FileName);
            }
        }

        class FileExtArg : ISystemArgProvider
        {
            private readonly IFormFile formFile;

            public FileExtArg(IFormFile formFile)
            {
                this.formFile = formFile;
            }
            public string Name => "ext";

            public string DefaultFormatter => string.Empty;

            public object GetValue()
            {
                return Path.GetExtension(formFile.FileName);
            }
        }
    }
}

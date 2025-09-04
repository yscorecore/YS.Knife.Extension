using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using YS.Knife.FileStorage;

namespace YS.Knife.FileStorage.Api.AspnetCore
{
    [Route("api/[controller]")]
    [ApiController]
    [AutoConstructor]
    public partial class FileController : ControllerBase
    {
        private readonly IFileCategoryProvider fileCategoryProvider;
        private readonly IDictionary<string, IFileStreamInterceptor> streamInterceptors;
        private readonly IDictionary<string, ISystemArgProvider> systemArgs;
        private readonly IDictionary<string, IFileUploadCallBack> callbacks;
        private readonly IServiceProvider serviceProvider;
        [HttpPost]
        [Route("upload/{category}")]
        public async Task<FileObject> Upload([FromRoute] string category)
        {
            var categoryObj = await fileCategoryProvider.CreateCategory(category) ?? throw new Exception($"The file category '{category}' is not defined");
            var formFile = this.Request.Form.Files[categoryObj.FileFormName] ?? throw new Exception($"Missing form file field '{categoryObj.FileFormName}'");
            var extName = Path.GetExtension(formFile.FileName);
            if (categoryObj.MaxLength > 0 && formFile.Length > categoryObj.MaxLength)
            {
                throw new Exception($"The uploaded file size exceeds the limit of {categoryObj.MaxLength} bytes.");
            }
            if (categoryObj.AllowExtensions != null && categoryObj.AllowExtensions.Length > 0 && !categoryObj.AllowExtensions.Contains(extName, StringComparer.InvariantCultureIgnoreCase))
            {
                throw new Exception($"The uploaded file extension '{extName}' is not allowed. Allowed extensions: {string.Join(", ", categoryObj.AllowExtensions)}");
            }

            var fileStorageService = serviceProvider.GetServiceByNameOrConfiguationSwitch<IFileStorageService>(categoryObj.ServiceName);

            //元数据
            var userArgs = this.Request.Query.ToDictionary(k => k.Key, v => v.Value.ToString());
            CheckUserArgs(userArgs);
            //增加系统参数
            systemArgs["ext"] = new FixedValueArgProvider(extName);

            var meta = new Dictionary<string, object>();
            foreach (var item in categoryObj.Metadata ?? new Dictionary<string, object>())
            {
                if (item.Value is string strValue)
                {
                    meta[item.Key] = FillTemplate(strValue, userArgs, systemArgs);
                }
                else
                {
                    meta[item.Key] = item.Value;
                }
            }
            //文件名
            var fileName = FillTemplate(categoryObj.PathTemplate, userArgs, systemArgs);
            //处理流
            var stream = formFile.OpenReadStream();
            foreach (var interceptor in categoryObj.Interceptors ?? Array.Empty<string>())
            {
                stream = RunInterceptor(stream, interceptor, userArgs, systemArgs);
                stream.Position = 0;
            }
            var res = await fileStorageService.PutObject(fileName, stream, categoryObj.Metadata);
            //处理回调
            foreach (var callback in categoryObj.Callbacks ?? Array.Empty<string>())
            {
                await RunCallback(res, callback, meta);
            }
            return res;

        }
        private Stream RunInterceptor(Stream stream, string interceptorName, IDictionary<string, string> userArgs, IDictionary<string, ISystemArgProvider> systemArgs)
        {
            if (streamInterceptors.TryGetValue(interceptorName, out var interceptor))
            {
                return interceptor.HandlerStream(stream, userArgs, systemArgs, this.HttpContext.RequestAborted);
            }
            else
            {
                throw new Exception($"The file stream interceptor '{interceptorName}' is not defined");
            }
        }
        private async Task RunCallback(FileObject fileObject, string callbackName, IDictionary<string, object> meta)
        {
            if (callbacks.TryGetValue(callbackName, out var callbackHandler))
            {
                await callbackHandler.OnFileUploaded(fileObject, meta, this.HttpContext.RequestAborted);
            }
            else
            {
                throw new Exception($"The file upload callback '{callbackName}' is not defined");
            }
        }
        private string FillTemplate(string template, IDictionary<string, string> userArgs, IDictionary<string, ISystemArgProvider> systemArgs)
        {
            return TemplatePlaceholder.Instance.FillPlaceholder(template, userArgs, systemArgs);
        }

        private void CheckUserArgs(IDictionary<string, string> userArgs)
        {
            foreach (var (k, v) in userArgs)
            {
                if (!Regex.IsMatch(v, @"^\w+$"))
                {
                    throw new Exception($"The user argument '{k}' is invalid.");
                }
            }
        }
        //[HttpGet]
        //[Route("client-upload/{category}")]
        //public async Task<ClientUploadInfo> GetClientDirectlyUpoadInfo([FromRoute] string category, [FromQuery][RegularExpression(@"^\.\w+$")] string ext = ".tmp")
        //{
        //    var categoryObj = await fileCategoryProvider.CreateCategory(category) ?? throw new Exception($"The file category '{category}' is not defined");
        //    var fileStorageService = serviceProvider.GetServiceByNameOrConfiguationSwitch<IFileStorageService>(categoryObj.ServiceName);

        //    var userArgs = this.Request.Query.Where(p => p.Key != nameof(ext)).ToDictionary(k => k.Key, v => v.Value.ToString());
        //    CheckUserArgs(userArgs);
        //    //增加系统参数
        //    systemArgs["ext"] = new FixedValueArgProvider(ext);
        //    var meta = new Dictionary<string, object>();
        //    foreach (var item in categoryObj.Metadata ?? new Dictionary<string, object>())
        //    {
        //        if (item.Value is string strValue)
        //        {
        //            meta[item.Key] = FillTemplate(strValue, userArgs, systemArgs);
        //        }
        //        else
        //        {
        //            meta[item.Key] = item.Value;
        //        }
        //    }
        //    //文件名
        //    var fileName = FillTemplate(categoryObj.PathTemplate, userArgs, systemArgs);
        //    return await fileStorageService.GetClientUploadInfo(fileName, meta, categoryObj, this.HttpContext.RequestAborted)
        //}
    }
}

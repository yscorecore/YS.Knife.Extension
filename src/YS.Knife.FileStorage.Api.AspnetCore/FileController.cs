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
            if (!IsValidFileName(formFile.FileName))
            {
                throw new Exception($"The uploaded file name '{formFile.FileName}' is invalid.");
            }

            var fileStorageService = serviceProvider.GetServiceByNameOrConfiguationSwitch<IFileStorageService>(categoryObj.ServiceName);

            //元数据
            var userArgs = this.Request.Query.ToDictionary(k => k.Key, v => v.Value.ToString());
            CheckUserArgs(userArgs);
            //增加系统参数
            userArgs["ext"] = extName;
            userArgs["name"] = Path.GetFileNameWithoutExtension(formFile.FileName);
            userArgs["fullname"] = formFile.FileName;
            userArgs["size"] = formFile.Length.ToString();
            userArgs["contenttype"] = formFile.ContentType;

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
                await RunCallback(res, callback, userArgs);
            }
            return res;

        }
        static bool IsValidFileName(string fileName)
        {
            try
            {
                // 获取操作系统不允许在文件名中出现的字符数组
                char[] invalidChars = Path.GetInvalidFileNameChars();

                // 检查文件名中是否包含任何不允许的字符
                foreach (char c in invalidChars)
                {
                    if (fileName.IndexOf(c) >= 0)
                    {
                        return false;
                    }
                }

                // 检查文件名是否为空或仅包含空格
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    return false;
                }

                // 检查文件名是否包含保留文件名
                string[] reservedFileNames = { "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9" };
                foreach (string reservedName in reservedFileNames)
                {
                    if (fileName.Trim().Equals(reservedName, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private Stream RunInterceptor(Stream stream, string interceptorName, IDictionary<string, string> userArgs, IDictionary<string, ISystemArgProvider> systemArgs)
        {
            if (streamInterceptors.TryGetValue(interceptorName, out var interceptor))
            {
                return interceptor.HandlerStream(stream, userArgs, this.HttpContext.RequestAborted);
            }
            else
            {
                throw new Exception($"The file stream interceptor '{interceptorName}' is not defined");
            }
        }
        private async Task RunCallback(FileObject fileObject, string callbackName, IDictionary<string, string> userArgs)
        {
            if (callbacks.TryGetValue(callbackName, out var callbackHandler))
            {
                await callbackHandler.OnFileUploaded(fileObject, userArgs, this.HttpContext.RequestAborted);
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
                if (!string.IsNullOrEmpty(v) && !Regex.IsMatch(v, @"^\w+$"))
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

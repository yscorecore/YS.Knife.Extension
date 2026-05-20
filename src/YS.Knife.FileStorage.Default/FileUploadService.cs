using System;
using System.Text.RegularExpressions;

namespace YS.Knife.FileStorage.Default
{
    [Service(typeof(IFileUploadService))]
    [Service(typeof(FileUploadService))]
    [AutoConstructor]
    public partial class FileUploadService : IFileUploadService
    {
        private readonly IFileCategoryProvider fileCategoryProvider;
        private readonly IDictionary<string, IFileStreamInterceptor> streamInterceptors;
        private readonly IDictionary<string, ISystemArgProvider> systemArgs;
        private readonly IDictionary<string, IFileUploadCallBack> callbacks;
        private readonly ITemplatePlaceholder templatePlaceholder;
        private readonly IServiceProvider serviceProvider;

        internal async Task<FileUploadObject> Upload(string category, FileCategory categoryObj, Stream stream, string streamFileName, long streamLength, string streamContentType, IDictionary<string, string> userArgs, CancellationToken cancellationToken)
        {
            userArgs ??= new Dictionary<string, string>();
            var extName = Path.GetExtension(streamFileName);
            if (categoryObj.MaxLength > 0 && streamLength > categoryObj.MaxLength)
            {
                throw new Exception($"The uploaded file size exceeds the limit of {categoryObj.MaxLength} bytes.");
            }
            if (categoryObj.AllowExtensions != null && categoryObj.AllowExtensions.Length > 0 && !categoryObj.AllowExtensions.Contains(extName, StringComparer.InvariantCultureIgnoreCase))
            {
                throw new Exception($"The uploaded file extension '{extName}' is not allowed. Allowed extensions: {string.Join(", ", categoryObj.AllowExtensions)}");
            }
            if (!IsValidFileName(streamFileName))
            {
                throw new Exception($"The uploaded file name '{streamFileName}' is invalid.");
            }

            var fileStorageService = serviceProvider.GetServiceByNameOrConfiguationSwitch<IFileStorageService>(categoryObj.ServiceName);

            //元数据
            CheckUserArgs(userArgs);
            //增加系统参数
            userArgs["ext"] = extName;
            userArgs["name"] = Path.GetFileNameWithoutExtension(streamFileName);
            userArgs["fullname"] = streamFileName;
            userArgs["size"] = streamLength.ToString();
            userArgs["contenttype"] = streamContentType;
            userArgs["category"] = category;

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

            foreach (var interceptor in categoryObj.Interceptors ?? Array.Empty<string>())
            {
                stream = RunInterceptor(stream, interceptor, userArgs, systemArgs, cancellationToken);
                stream.Position = 0;
            }
            var res = await fileStorageService.PutObject(fileName, stream, categoryObj.Metadata);
            //处理回调
            foreach (var callback in categoryObj.Callbacks ?? Array.Empty<string>())
            {
                await RunCallback(res, callback, userArgs, cancellationToken);
            }
            return new FileUploadObject
            {
                FileName = res.FileName,
                Extension = extName,
                Key = res.Key,
                PublicUrl = res.PublicUrl,
                Size = streamLength,
                OriginFileName = streamFileName
            };
        }
        public async Task<FileUploadObject> Upload(string category, Stream stream, string streamFileName, long streamLength, string streamContentType, IDictionary<string, string> userArgs, CancellationToken cancellationToken)
        {
            var categoryObj = await fileCategoryProvider.CreateCategory(category) ?? throw new Exception($"The file category '{category}' is not defined");
            return await Upload(category, categoryObj, stream, streamFileName, streamLength, streamContentType, userArgs, cancellationToken);
        }

        private Stream RunInterceptor(Stream stream, string interceptorName, IDictionary<string, string> userArgs, IDictionary<string, ISystemArgProvider> systemArgs, CancellationToken token)
        {
            if (streamInterceptors.TryGetValue(interceptorName, out var interceptor))
            {
                return interceptor.HandlerStream(stream, userArgs, token);
            }
            else
            {
                throw new Exception($"The file stream interceptor '{interceptorName}' is not defined");
            }
        }
        private async Task RunCallback(FileObject fileObject, string callbackName, IDictionary<string, string> userArgs, CancellationToken token)
        {
            if (callbacks.TryGetValue(callbackName, out var callbackHandler))
            {
                await callbackHandler.OnFileUploaded(fileObject, userArgs, token);
            }
            else
            {
                throw new Exception($"The file upload callback '{callbackName}' is not defined");
            }
        }
        private string FillTemplate(string template, IDictionary<string, string> userArgs, IDictionary<string, ISystemArgProvider> systemArgs)
        {
            return templatePlaceholder.FillPlaceholder(template, userArgs, systemArgs);
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
    }
}

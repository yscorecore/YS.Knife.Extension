using System.Text.RegularExpressions;
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
        private readonly IServiceProvider serviceProvider;
        [HttpPost]
        [Route("upload/{category}")]
        public async Task<FileObject> Upload([FromRoute] string category)
        {

            var categoryObj = await fileCategoryProvider.CreateCategory(category) ?? throw new Exception($"The file category '{category}' is not defined");
            var formFile = this.Request.Form.Files[categoryObj.FileFormName] ?? throw new Exception($"Missing form file field '{categoryObj.FileFormName}'");
            var extName = Path.GetExtension(formFile.FileName);
            var name = Path.GetFileNameWithoutExtension(formFile.FileName);
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
            systemArgs["name"] = new FixedValueArgProvider(name);
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
            return await fileStorageService.PutObject(fileName, stream, categoryObj.Metadata);

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
    }


    public static class ServiceExtensions
    {
        public static T GetServiceByName<T>(this IServiceProvider serviceProvider, string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return serviceProvider.GetRequiredService<T>();
            }
            else
            {
                var services = serviceProvider.GetRequiredService<IDictionary<string, T>>();
                if (services.TryGetValue(name, out var s))
                {
                    return s;
                }
                throw new Exception($"Can not find service '{typeof(T).FullName}' by name '{name}'");
            }
        }
        public static T GetServiceByNameOrConfiguationSwitch<T>(this IServiceProvider serviceProvider, string name = default)
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var serviceName = name ??
                configuration[$"ServiceSwitch:{typeof(T).Name}"]
                ?? configuration[$"ServiceSwitch:{typeof(T).FullName}"];
            return GetServiceByName<T>(serviceProvider, serviceName);
        }
        public static T GetServiceByConfiguationSwitch<T>(this IServiceProvider serviceProvider)
        {
            return GetServiceByNameOrConfiguationSwitch<T>(serviceProvider, null);
        }
    }
}

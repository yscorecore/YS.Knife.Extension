using Microsoft.AspNetCore.Mvc;

namespace YS.Knife.FileStorage.Api.AspnetCore
{
    [Route("api/[controller]")]
    [ApiController]
    [AutoConstructor]
    public partial class FileController : ControllerBase
    {

        private readonly IFileUploadWebService fileuploadWebService;
        [HttpPost]
        [Route("upload/{category}")]
        public Task<FileUploadObject> Upload([FromRoute] string category)
        {
            return fileuploadWebService.Upload(category, HttpContext.RequestAborted);

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

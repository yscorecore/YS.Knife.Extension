using Microsoft.AspNetCore.Mvc;
using YS.Knife.FileStorage;

namespace YS.Knife.DataSource.Management
{
    [Route("api/[controller]")]
    [ApiController]
    [AutoConstructor]
    public partial class FileController : ControllerBase
    {

        private readonly IFileCategoryFactory fileCategoryFactory;
        private readonly IFileStorageService fileStorageService;
        [HttpPost]
        [Route("upload/{category}")]
        public async Task<FileObject> Upload([FromRoute] string category)
        {
            var categoryObj = await fileCategoryFactory.CreateCategory(category);
            var formFile = this.Request.Form.Files[categoryObj.FileFormName];
            if (formFile == null)
            {
                throw new Exception($"Missing form file field '{categoryObj.FileFormName}'");
            }
            //MergeVarsToCategory(meta,categoryObj.Vars);
            return await fileStorageService.PutObject("", formFile.OpenReadStream(), categoryObj.Metadata);
            //void MergeVarsToCategory(IDictionary<string,string> data, IDictionary<string,object> vars )
            //{
            //    foreach (var item in this.Request.Form)
            //    {
            //        if (categoryObj.Vars.ContainsKey(item.Key))
            //        {
            //            throw new Exception($"The form field '{item.Key}' can not be setted");
            //        }
            //        else
            //        { 

            //        }   
            //    }
            //}
        }

    }
}

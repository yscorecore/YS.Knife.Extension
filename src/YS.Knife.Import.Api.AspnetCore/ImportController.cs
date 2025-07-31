using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using YS.Knife.Import.Abstractions;

namespace YS.Knife.Import.Api.AspnetCore
{
    [ApiController]
    [Route("import")]
    [AutoConstructor]
    public partial class ImportController : ControllerBase
    {
        private readonly IImportService importService;

        [HttpPost]
        [Route("begin")]
        public Task<ImportToken> BeginImport([Required][FromForm] IFormFile file)
        {
            using var stream = file.OpenReadStream();
            var fileExt = Path.GetExtension(file.FileName);
            return importService.BeginImport(stream, fileExt);
        }
        [HttpGet]
        [Route("read-column")]
        public Task<Dictionary<string, List<ColumnInfo>>> ReadColumns(Guid token)
        {
            return importService.ReadColumns(token);
        }
        [HttpGet]
        [Route("complete")]
        public Task<bool> EndImport(Guid token)
        {
            return importService.EndImport(token);
        }
        [HttpPost]
        [Route("read-data")]
        public Task<Dictionary<string, List<Dictionary<string, object>>>> ReadData([FromQuery] Guid token, [FromBody] EntityMetadata[] entityMetadata)
        {
            return importService.ReadData(token, entityMetadata);
        }
    }
}

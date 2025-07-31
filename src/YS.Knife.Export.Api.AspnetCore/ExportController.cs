using Microsoft.AspNetCore.Mvc;

namespace YS.Knife.Export.Api.AspnetCore
{
    [ApiController]
    [Route("export")]
    [AutoConstructor]
    public partial class ExportController : ControllerBase
    {
        private readonly IExportService exportService;
        [HttpPost]
        [Route("begin")]
        public Task<ExportToken> BeginExport(EntityMetadata[] metadatas)
        {
            return exportService.BeginExport(metadatas);
        }
        [HttpGet]
        [Route("cancel")]
        public Task<bool> CancelExport([FromQuery] Guid token)
        {
            return exportService.CancelExport(token);
        }
        [HttpGet]
        [Route("complete")]
        public async Task<IActionResult> EndExport([FromQuery] Guid token, [FromQuery] string fileName)
        {
            var stream = await exportService.EndExport(token);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        [HttpPost]
        [Route("append-data")]
        public Task Export([FromQuery] Guid token, [FromBody] EntityData data)
        {
            return exportService.Export(token, data);
        }
    }
}

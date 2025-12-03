using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace YS.Knife.FileManager.Api.AspnetCore
{
    [ApiController]
    [Route("api/[controller]")]
    [AutoConstructor]
    public partial class CloudFileController : ControllerBase
    {
        private readonly ICloudFileManagerService cloudFileManagerService;
        [HttpPost]
        public Task<Guid[]> Create([FromBody] CreateFileDto<Guid>[] dtos, CancellationToken token = default)
        {
            return cloudFileManagerService.Create(dtos, token);
        }
        [HttpPost]
        [Route("folders")]
        public Task<Guid[]> CreateFolder([FromBody] CreateFolderDto<Guid>[] dtos, CancellationToken token = default)
        {
            return cloudFileManagerService.CreateFolder(dtos, token);
        }
        [HttpDelete]
        public Task Delete(Guid[] id, CancellationToken token = default)
        {
            return cloudFileManagerService.Delete(id);
        }
        [HttpPost]
        [Route("rename")]
        public Task Rename([FromBody] RenameFileDto<Guid> renameFileDto, CancellationToken cancellationToken = default)
        {
            return cloudFileManagerService.Rename(renameFileDto, cancellationToken);
        }
    }
}

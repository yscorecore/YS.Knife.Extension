using FlyTiger;
using Microsoft.AspNetCore.Mvc;

namespace YS.Knife.Metadata.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [AutoConstructor]
    public partial class MetadataController : ControllerBase, IMetadataService
    {
        private readonly IMetadataService metadataService;
        [HttpGet]
        [Route("{name}")]
        public Task<MetadataInfo> GetMetadataInfo([FromRoute] string name, CancellationToken cancellationToken = default)
        {
            return metadataService.GetMetadataInfo(name, cancellationToken);
        }
        [HttpGet]
        [Route("list-all")]
        public Task<List<string>> ListAllNames(CancellationToken cancellationToken = default)
        {
            return metadataService.ListAllNames(cancellationToken);
        }
    }
}

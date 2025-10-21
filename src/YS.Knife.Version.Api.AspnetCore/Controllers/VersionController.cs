using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace YS.Knife.Version.Api.AspnetCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AutoConstructor]
    public partial class VersionController : ControllerBase
    {
        private readonly IVersionService versionService;

        [HttpGet]
        public VersionInfo GetVersionInfo()
        {
            return versionService.GetVersionInfo();
        }
    }
}

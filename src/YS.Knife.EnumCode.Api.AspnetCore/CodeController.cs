using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static YS.Knife.EnumCode.IEnumCodeService;

namespace YS.Knife.EnumCode.AspnetCore
{

    [ApiController]
    [Route("codes")]
    [AutoConstructor]
    public partial class CodeController : ControllerBase
    {
        private IEnumCodeService codeService;
        [HttpGet]
        [Route("all")]
        [AllowAnonymous]
        public Task<Dictionary<string, List<CodeInfo>>> GetAllCodes()
        {
            return codeService.GetAllCodes();
        }
    }
}

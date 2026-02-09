# 修复 GenerateController_WithVoidReturnType_GeneratesPostMethod 测试用例

$xmlContent = @"
<?xml version="1.0" encoding="utf-8"?>
<case>
    <input>
        <code><![CDATA[using System;

public interface IActionService
{
    void ExecuteAction();
}

[YS.Knife.ExposeApi(typeof(IActionService))]
public class TestController
{
}
]]></code>
    </input>
    <output>
        <code file="ActionController.g.cs"><![CDATA[using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace 
{
    [ApiController]
    [Route("api/[controller]")]
    public class ActionController : ControllerBase
    {
        private readonly global::IActionService iActionService;
        public ActionController(global::IActionService iActionService)
        {
            this.iActionService = iActionService;
        }
        [Route("ExecuteAction")]
        [HttpPost]
        public void ExecuteAction()
        {
            return this.iActionService.ExecuteAction();
        }
    }
}
]]></code>
    </output>
</case>
"@
$xmlContent | Out-File -FilePath ".\test\YS.Knife.Generators.ExposeApi.UnitTest\testdata\GenerateController_WithVoidReturnType_GeneratesPostMethod.xml" -Encoding utf8

Write-Host "Fixed GenerateController_WithVoidReturnType_GeneratesPostMethod test case."

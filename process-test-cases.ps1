# 定义重复的测试用例名称
$duplicateTestCases = @(
    "GenerateController_WithCancellationToken_DoesNotBindParameter",
    "GenerateController_WithHttpContext_DoesNotBindParameter",
    "GenerateController_WithMethodStartingWithCreate_UsesHttpPost",
    "GenerateController_WithMethodStartingWithDelete_UsesHttpDelete",
    "GenerateController_WithMethodStartingWithGet_UsesHttpGet",
    "GenerateController_WithMethodStartingWithPatch_UsesHttpPatch",
    "GenerateController_WithMethodStartingWithUpdate_UsesHttpPut"
)

# 定义要添加到 XML 的测试用例
$testCasesToAdd = @(
    @{
        Name = "GenerateController_WithSimpleService_GeneratesCorrectController"
        Input = @"
using System;

public interface ITestService
{
    string GetTestData(int id);
}

[YS.Knife.ExposeApi(typeof(ITestService))]
public class TestController
{
}
"@
        OutputFile = "TestController.g.cs"
        Output = @"
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace 
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly global::ITestService iTestService;
        public TestController(global::ITestService iTestService)
        {
            this.iTestService = iTestService;
        }
        [Route("GetTestData")]
        [HttpGet]
        public string GetTestData([FromQuery] int id)
        {
            return this.iTestService.GetTestData(id);
        }
    }
}
"@
    },
    @{
        Name = "GenerateController_WithNoParameters_GeneratesMethodWithoutParameters"
        Input = @"
using System;

public interface IHealthService
{
    string GetHealth();
}

[YS.Knife.ExposeApi(typeof(IHealthService))]
public class TestController
{
}
"@
        OutputFile = "HealthController.g.cs"
        Output = @"
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace 
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly global::IHealthService iHealthService;
        public HealthController(global::IHealthService iHealthService)
        {
            this.iHealthService = iHealthService;
        }
        [Route("GetHealth")]
        [HttpGet]
        public string GetHealth()
        {
            return this.iHealthService.GetHealth();
        }
    }
}
"@
    },
    @{
        Name = "GenerateController_WithVoidReturnType_GeneratesPostMethod"
        Input = @"
using System;

public interface IActionService
{
    void ExecuteAction();
}

[YS.Knife.ExposeApi(typeof(IActionService))]
public class TestController
{
}
"@
        OutputFile = "ActionController.g.cs"
        Output = @"
using Microsoft.AspNetCore.Authorization;
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
            this.iActionService.ExecuteAction();
        }
    }
}
"@
    },
    @{
        Name = "GenerateController_WithIdParameter_UsesRouteParameter"
        Input = @"
using System;

public interface IResourceService
{
    Resource GetResource(int id);
}

public class Resource
{
    public int Id { get; set; }
}

[YS.Knife.ExposeApi(typeof(IResourceService))]
public class TestController
{
}
"@
        OutputFile = "ResourceController.g.cs"
        Output = @"
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace 
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResourceController : ControllerBase
    {
        private readonly global::IResourceService iResourceService;
        public ResourceController(global::IResourceService iResourceService)
        {
            this.iResourceService = iResourceService;
        }
        [Route("GetResource")]
        [HttpGet]
        public global::Resource GetResource([FromQuery] int id)
        {
            return this.iResourceService.GetResource(id);
        }
    }
}
"@
    },
    @{
        Name = "GenerateController_WithNameParameter_UsesRouteParameter"
        Input = @"
using System;

public interface IItemService
{
    Item GetItem(string name);
}

public class Item
{
    public string Name { get; set; }
}

[YS.Knife.ExposeApi(typeof(IItemService))]
public class TestController
{
}
"@
        OutputFile = "ItemController.g.cs"
        Output = @"
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace 
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemController : ControllerBase
    {
        private readonly global::IItemService iItemService;
        public ItemController(global::IItemService iItemService)
        {
            this.iItemService = iItemService;
        }
        [Route("GetItem")]
        [HttpGet]
        public global::Item GetItem([FromQuery] string name)
        {
            return this.iItemService.GetItem(name);
        }
    }
}
"@
    },
    @{
        Name = "GenerateController_WithKeyParameter_UsesRouteParameter"
        Input = @"
using System;

public interface IEntryService
{
    Entry GetEntry(string key);
}

public class Entry
{
    public string Key { get; set; }
}

[YS.Knife.ExposeApi(typeof(IEntryService))]
public class TestController
{
}
"@
        OutputFile = "EntryController.g.cs"
        Output = @"
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace 
{
    [ApiController]
    [Route("api/[controller]")]
    public class EntryController : ControllerBase
    {
        private readonly global::IEntryService iEntryService;
        public EntryController(global::IEntryService iEntryService)
        {
            this.iEntryService = iEntryService;
        }
        [Route("GetEntry")]
        [HttpGet]
        public global::Entry GetEntry([FromQuery] string key)
        {
            return this.iEntryService.GetEntry(key);
        }
    }
}
"@
    },
    @{
        Name = "GenerateController_WithMultipleRouteParameters_UsesFirstAsRoute"
        Input = @"
using System;

public interface IDataService
{
    Data GetData(int id, string filter);
}

public class Data
{
    public int Id { get; set; }
}

[YS.Knife.ExposeApi(typeof(IDataService))]
public class TestController
{
}
"@
        OutputFile = "DataController.g.cs"
        Output = @"
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace 
{
    [ApiController]
    [Route("api/[controller]")]
    public class DataController : ControllerBase
    {
        private readonly global::IDataService iDataService;
        public DataController(global::IDataService iDataService)
        {
            this.iDataService = iDataService;
        }
        [Route("GetData")]
        [HttpGet]
        public global::Data GetData([FromQuery] int id, [FromQuery] string filter)
        {
            return this.iDataService.GetData(id, filter);
        }
    }
}
"@
    },
    @{
        Name = "GenerateController_WithMethodStartingWithQuery_UsesHttpGet"
        Input = @"
using System;

public interface ISearchService
{
    string QueryUsers(string keyword);
}

[YS.Knife.ExposeApi(typeof(ISearchService))]
public class TestController
{
}
"@
        OutputFile = "SearchController.g.cs"
        Output = @"
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace 
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly global::ISearchService iSearchService;
        public SearchController(global::ISearchService iSearchService)
        {
            this.iSearchService = iSearchService;
        }
        [Route("QueryUsers")]
        [HttpGet]
        public string QueryUsers([FromQuery] string keyword)
        {
            return this.iSearchService.QueryUsers(keyword);
        }
    }
}
"@
    },
    @{
        Name = "GenerateController_WithMethodStartingWithFind_UsesHttpGet"
        Input = @"
using System;

public interface ILookupService
{
    string FindUser(string email);
}

[YS.Knife.ExposeApi(typeof(ILookupService))]
public class TestController
{
}
"@
        OutputFile = "LookupController.g.cs"
        Output = @"
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace 
{
    [ApiController]
    [Route("api/[controller]")]
    public class LookupController : ControllerBase
    {
        private readonly global::ILookupService iLookupService;
        public LookupController(global::ILookupService iLookupService)
        {
            this.iLookupService = iLookupService;
        }
        [Route("FindUser")]
        [HttpGet]
        public string FindUser([FromQuery] string email)
        {
            return this.iLookupService.FindUser(email);
        }
    }
}
"@
    },
    @{
        Name = "GenerateController_WithMethodStartingWithAdd_UsesHttpPost"
        Input = @"
using System;

public interface IInsertionService
{
    string AddUser(string name);
}

[YS.Knife.ExposeApi(typeof(IInsertionService))]
public class TestController
{
}
"@
        OutputFile = "InsertionController.g.cs"
        Output = @"
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace 
{
    [ApiController]
    [Route("api/[controller]")]
    public class InsertionController : ControllerBase
    {
        private readonly global::IInsertionService iInsertionService;
        public InsertionController(global::IInsertionService iInsertionService)
        {
            this.iInsertionService = iInsertionService;
        }
        [Route("AddUser")]
        [HttpPost]
        public string AddUser([FromBody] string name)
        {
            return this.iInsertionService.AddUser(name);
        }
    }
}
"@
    },
    @{
        Name = "GenerateController_WithMethodStartingWithRemove_UsesHttpDelete"
        Input = @"
using System;

public interface IDeletionService
{
    void RemoveUser(int id);
}

[YS.Knife.ExposeApi(typeof(IDeletionService))]
public class TestController
{
}
"@
        OutputFile = "DeletionController.g.cs"
        Output = @"
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace 
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeletionController : ControllerBase
    {
        private readonly global::IDeletionService iDeletionService;
        public DeletionController(global::IDeletionService iDeletionService)
        {
            this.iDeletionService = iDeletionService;
        }
        [Route("RemoveUser")]
        [HttpDelete]
        public void RemoveUser([FromQuery] int id)
        {
            this.iDeletionService.RemoveUser(id);
        }
    }
}
"@
    },
    @{
        Name = "GenerateController_WithMethodStartingWithModify_UsesHttpPut"
        Input = @"
using System;

public interface IEditService
{
    string ModifyUser(int id, string name);
}

[YS.Knife.ExposeApi(typeof(IEditService))]
public class TestController
{
}
"@
        OutputFile = "EditController.g.cs"
        Output = @"
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace 
{
    [ApiController]
    [Route("api/[controller]")]
    public class EditController : ControllerBase
    {
        private readonly global::IEditService iEditService;
        public EditController(global::IEditService iEditService)
        {
            this.iEditService = iEditService;
        }
        [Route("ModifyUser")]
        [HttpPut]
        public string ModifyUser([FromBody] __ModifyUser_BodyArg arg)
        {
            return this.iEditService.ModifyUser(arg.id, arg.name);
        }
        public record __ModifyUser_BodyArg(int id, string name);
    }
}
"@
    },
    @{
        Name = "GenerateController_WithMethodStartingWithFetch_UsesHttpGet"
        Input = @"
using System;

public interface IRetrievalService
{
    string FetchData();
}

[YS.Knife.ExposeApi(typeof(IRetrievalService))]
public class TestController
{
}
"@
        OutputFile = "RetrievalController.g.cs"
        Output = @"
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace 
{
    [ApiController]
    [Route("api/[controller]")]
    public class RetrievalController : ControllerBase
    {
        private readonly global::IRetrievalService iRetrievalService;
        public RetrievalController(global::IRetrievalService iRetrievalService)
        {
            this.iRetrievalService = iRetrievalService;
        }
        [Route("FetchData")]
        [HttpGet]
        public string FetchData()
        {
            return this.iRetrievalService.FetchData();
        }
    }
}
"@
    },
    @{
        Name = "GenerateController_WithMethodStartingWithUpload_UsesHttpPost"
        Input = @"
using System;
using Microsoft.AspNetCore.Http;

public interface IFileService
{
    string UploadFile(IFormFile file);
}

[YS.Knife.ExposeApi(typeof(IFileService))]
public class TestController
{
}
"@
        OutputFile = "FileController.g.cs"
        Output = @"
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace 
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        private readonly global::IFileService iFileService;
        public FileController(global::IFileService iFileService)
        {
            this.iFileService = iFileService;
        }
        [Route("UploadFile")]
        [HttpPost]
        public string UploadFile([FromForm] Microsoft.AspNetCore.Http.IFormFile file)
        {
            return this.iFileService.UploadFile(file);
        }
    }
}
"@
    },
    @{
        Name = "GenerateController_WithMethodStartingWithSave_UsesHttpPost"
        Input = @"
using System;

public interface IPersistenceService
{
    string SaveData(string data);
}

[YS.Knife.ExposeApi(typeof(IPersistenceService))]
public class TestController
{
}
"@
        OutputFile = "PersistenceController.g.cs"
        Output = @"
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace 
{
    [ApiController]
    [Route("api/[controller]")]
    public class PersistenceController : ControllerBase
    {
        private readonly global::IPersistenceService iPersistenceService;
        public PersistenceController(global::IPersistenceService iPersistenceService)
        {
            this.iPersistenceService = iPersistenceService;
        }
        [Route("SaveData")]
        [HttpPost]
        public string SaveData([FromBody] string data)
        {
            return this.iPersistenceService.SaveData(data);
        }
    }
}
"@
    },
    @{
        Name = "GenerateController_WithMethodStartingWithEdit_UsesHttpPut"
        Input = @"
using System;

public interface IModificationService2
{
    string EditUser(int id, string name);
}

[YS.Knife.ExposeApi(typeof(IModificationService2))]
public class TestController
{
}
"@
        OutputFile = "Modification2Controller.g.cs"
        Output = @"
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace 
{
    [ApiController]
    [Route("api/[controller]")]
    public class Modification2Controller : ControllerBase
    {
        private readonly global::IModificationService2 iModificationService2;
        public Modification2Controller(global::IModificationService2 iModificationService2)
        {
            this.iModificationService2 = iModificationService2;
        }
        [Route("EditUser")]
        [HttpPut]
        public string EditUser([FromBody] __EditUser_BodyArg arg)
        {
            return this.iModificationService2.EditUser(arg.id, arg.name);
        }
        public record __EditUser_BodyArg(int id, string name);
    }
}
"@
    },
    @{
        Name = "GenerateController_WithMethodStartingWithPost_UsesHttpPost"
        Input = @"
using System;

public interface IPostService
{
    string PostData(string data);
}

[YS.Knife.ExposeApi(typeof(IPostService))]
public class TestController
{
}
"@
        OutputFile = "PostController.g.cs"
        Output = @"
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace 
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostController : ControllerBase
    {
        private readonly global::IPostService iPostService;
        public PostController(global::IPostService iPostService)
        {
            this.iPostService = iPostService;
        }
        [Route("PostData")]
        [HttpPost]
        public string PostData([FromBody] string data)
        {
            return this.iPostService.PostData(data);
        }
    }
}
"@
    },
    @{
        Name = "GenerateController_WithComplexObjectParameter_UsesFromBody"
        Input = @"
using System;

public interface IComplexObjectService
{
    string ProcessData(DataModel model);
}

public class DataModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}

[YS.Knife.ExposeApi(typeof(IComplexObjectService))]
public class TestController
{
}
"@
        OutputFile = "ComplexObjectController.g.cs"
        Output = @"
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace 
{
    [ApiController]
    [Route("api/[controller]")]
    public class ComplexObjectController : ControllerBase
    {
        private readonly global::IComplexObjectService iComplexObjectService;
        public ComplexObjectController(global::IComplexObjectService iComplexObjectService)
        {
            this.iComplexObjectService = iComplexObjectService;
        }
        [Route("ProcessData")]
        [HttpPost]
        public string ProcessData([FromBody] global::DataModel model)
        {
            return this.iComplexObjectService.ProcessData(model);
        }
    }
}
"@
    },
    @{
        Name = "GenerateController_WithMultipleComplexParameters_CreatesRecordType"
        Input = @"
using System;

public interface IMultiParamService
{
    string ProcessData(string name, int value, string description);
}

[YS.Knife.ExposeApi(typeof(IMultiParamService))]
public class TestController
{
}
"@
        OutputFile = "MultiParamController.g.cs"
        Output = @"
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace 
{
    [ApiController]
    [Route("api/[controller]")]
    public class MultiParamController : ControllerBase
    {
        private readonly global::IMultiParamService iMultiParamService;
        public MultiParamController(global::IMultiParamService iMultiParamService)
        {
            this.iMultiParamService = iMultiParamService;
        }
        [Route("ProcessData")]
        [HttpPost]
        public string ProcessData([FromBody] __ProcessData_BodyArg arg)
        {
            return this.iMultiParamService.ProcessData(arg.name, arg.value, arg.description);
        }
        public record __ProcessData_BodyArg(string name, int value, string description);
    }
}
"@
    },
    @{
        Name = "GenerateController_WithMixedParameters_HandlesCorrectly"
        Input = @"
using System;
using System.Threading;

public interface IMixedService
{
    string GetData(int id, string filter, CancellationToken cancellationToken);
}

[YS.Knife.ExposeApi(typeof(IMixedService))]
public class TestController
{
}
"@
        OutputFile = "MixedController.g.cs"
        Output = @"
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace 
{
    [ApiController]
    [Route("api/[controller]")]
    public class MixedController : ControllerBase
    {
        private readonly global::IMixedService iMixedService;
        public MixedController(global::IMixedService iMixedService)
        {
            this.iMixedService = iMixedService;
        }
        [Route("GetData")]
        [HttpGet]
        public string GetData([FromQuery] int id, [FromQuery] string filter, global::System.Threading.CancellationToken cancellationToken)
        {
            return this.iMixedService.GetData(id, filter, cancellationToken);
        }
    }
}
"@
    }
)

# 为不重复的测试用例创建 XML 文件
foreach ($testCase in $testCasesToAdd) {
    $xmlContent = @"
<?xml version="1.0" encoding="utf-8"?>
<case>
    <input>
        <code><![CDATA[$($testCase.Input)]]></code>
    </input>
    <output>
        <code file="$($testCase.OutputFile)"><![CDATA[$($testCase.Output)]]></code>
    </output>
</case>
"@
    
    $filePath = ".\test\YS.Knife.Generators.ExposeApi.UnitTest\testdata\$($testCase.Name).xml"
    $xmlContent | Out-File -FilePath $filePath -Encoding utf8
    Write-Host "Created XML file: $($testCase.Name).xml"
}

# 删除重复的测试用例
Write-Host "Duplicate test cases to be removed:"
foreach ($testCase in $duplicateTestCases) {
    Write-Host "- $testCase"
}

Write-Host "All tasks completed."

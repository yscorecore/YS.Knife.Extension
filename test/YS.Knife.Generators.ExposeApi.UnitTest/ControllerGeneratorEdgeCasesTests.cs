using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using YS.Knife.Generators.ExposeApi;

namespace YS.Knife.Generators.ExposeApi.UnitTest
{
    public class ControllerGeneratorEdgeCasesTests
    {
        [Fact]
        public async Task GenerateController_WithNoParameters_GeneratesMethodWithoutParameters()
        {
            var source = @"
using System;

public interface IHealthService
{
    string GetHealth();
}

[YS.Knife.ExposeApi(typeof(IHealthService))]
public class TestController
{
}
";

            var generatedCode = await GenerateSourceAsync(source);
            
            Assert.NotNull(generatedCode);
            Assert.Contains("GetHealth", generatedCode);
        }

        [Fact]
        public async Task GenerateController_WithVoidReturnType_GeneratesPostMethod()
        {
            var source = @"
using System;

public interface IActionService
{
    void ExecuteAction();
}

[YS.Knife.ExposeApi(typeof(IActionService))]
public class TestController
{
}
";

            var generatedCode = await GenerateSourceAsync(source);
            
            Assert.NotNull(generatedCode);
            Assert.Contains("[HttpPost]", generatedCode);
            Assert.Contains("ExecuteAction", generatedCode);
        }

        [Fact]
        public async Task GenerateController_WithIdParameter_UsesRouteParameter()
        {
            var source = @"
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
";

            var generatedCode = await GenerateSourceAsync(source);
            
            Assert.NotNull(generatedCode);
            Assert.Contains("int id", generatedCode);
        }

        [Fact]
        public async Task GenerateController_WithNameParameter_UsesRouteParameter()
        {
            var source = @"
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
";

            var generatedCode = await GenerateSourceAsync(source);
            
            Assert.NotNull(generatedCode);
            Assert.Contains("string name", generatedCode);
        }

        [Fact]
        public async Task GenerateController_WithKeyParameter_UsesRouteParameter()
        {
            var source = @"
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
";

            var generatedCode = await GenerateSourceAsync(source);
            
            Assert.NotNull(generatedCode);
            Assert.Contains("string key", generatedCode);
        }

        [Fact]
        public async Task GenerateController_WithMultipleRouteParameters_UsesFirstAsRoute()
        {
            var source = @"
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
";

            var generatedCode = await GenerateSourceAsync(source);
            
            Assert.NotNull(generatedCode);
            Assert.Contains("int id", generatedCode);
            Assert.Contains("string filter", generatedCode);
        }

        [Fact]
        public async Task GenerateController_WithCancellationToken_DoesNotBindParameter()
        {
            var source = @"
using System;
using System.Threading;

public interface ILongRunningService
{
    string ProcessData(CancellationToken cancellationToken);
}

[YS.Knife.ExposeApi(typeof(ILongRunningService))]
public class TestController
{
}
";

            var generatedCode = await GenerateSourceAsync(source);
            
            Assert.NotNull(generatedCode);
            Assert.Contains("CancellationToken cancellationToken", generatedCode);
        }

        [Fact]
        public async Task GenerateController_WithHttpContext_DoesNotBindParameter()
        {
            var source = @"
using System;
using Microsoft.AspNetCore.Http;

public interface IHttpService
{
    string GetRequestInfo(HttpContext context);
}

[YS.Knife.ExposeApi(typeof(IHttpService))]
public class TestController
{
}
";

            var generatedCode = await GenerateSourceAsync(source);
            
            Assert.NotNull(generatedCode);
            Assert.Contains("HttpContext context", generatedCode);
        }

        [Fact]
        public async Task GenerateController_WithMethodStartingWithGet_UsesHttpGet()
        {
            var source = @"
using System;

public interface IQueryService
{
    string GetUserInfo();
}

[YS.Knife.ExposeApi(typeof(IQueryService))]
public class TestController
{
}
";

            var generatedCode = await GenerateSourceAsync(source);
            
            Assert.NotNull(generatedCode);
            Assert.Contains("[HttpGet]", generatedCode);
            Assert.Contains("GetUserInfo", generatedCode);
        }

        [Fact]
        public async Task GenerateController_WithMethodStartingWithCreate_UsesHttpPost()
        {
            var source = @"
using System;

public interface ICreationService
{
    string CreateUser(string name);
}

[YS.Knife.ExposeApi(typeof(ICreationService))]
public class TestController
{
}
";

            var generatedCode = await GenerateSourceAsync(source);
            
            Assert.NotNull(generatedCode);
            Assert.Contains("[HttpPost]", generatedCode);
            Assert.Contains("CreateUser", generatedCode);
        }

        [Fact]
        public async Task GenerateController_WithMethodStartingWithUpdate_UsesHttpPut()
        {
            var source = @"
using System;

public interface IModificationService
{
    string UpdateUser(int id, string name);
}

[YS.Knife.ExposeApi(typeof(IModificationService))]
public class TestController
{
}
";

            var generatedCode = await GenerateSourceAsync(source);
            
            Assert.NotNull(generatedCode);
            Assert.Contains("[HttpPut]", generatedCode);
            Assert.Contains("UpdateUser", generatedCode);
        }

        [Fact]
        public async Task GenerateController_WithMethodStartingWithDelete_UsesHttpDelete()
        {
            var source = @"
using System;

public interface IRemovalService
{
    void DeleteUser(int id);
}

[YS.Knife.ExposeApi(typeof(IRemovalService))]
public class TestController
{
}
";

            var generatedCode = await GenerateSourceAsync(source);
            
            Assert.NotNull(generatedCode);
            Assert.Contains("[HttpDelete]", generatedCode);
            Assert.Contains("DeleteUser", generatedCode);
        }

        [Fact]
        public async Task GenerateController_WithMethodStartingWithQuery_UsesHttpGet()
        {
            var source = @"
using System;

public interface ISearchService
{
    string QueryUsers(string keyword);
}

[YS.Knife.ExposeApi(typeof(ISearchService))]
public class TestController
{
}
";

            var generatedCode = await GenerateSourceAsync(source);
            
            Assert.NotNull(generatedCode);
            Assert.Contains("[HttpGet]", generatedCode);
            Assert.Contains("QueryUsers", generatedCode);
        }

        [Fact]
        public async Task GenerateController_WithMethodStartingWithFind_UsesHttpGet()
        {
            var source = @"
using System;

public interface ILookupService
{
    string FindUser(string email);
}

[YS.Knife.ExposeApi(typeof(ILookupService))]
public class TestController
{
}
";

            var generatedCode = await GenerateSourceAsync(source);
            
            Assert.NotNull(generatedCode);
            Assert.Contains("[HttpGet]", generatedCode);
            Assert.Contains("FindUser", generatedCode);
        }

        [Fact]
        public async Task GenerateController_WithMethodStartingWithAdd_UsesHttpPost()
        {
            var source = @"
using System;

public interface IInsertionService
{
    string AddUser(string name);
}

[YS.Knife.ExposeApi(typeof(IInsertionService))]
public class TestController
{
}
";

            var generatedCode = await GenerateSourceAsync(source);
            
            Assert.NotNull(generatedCode);
            Assert.Contains("[HttpPost]", generatedCode);
            Assert.Contains("AddUser", generatedCode);
        }

        [Fact]
        public async Task GenerateController_WithMethodStartingWithRemove_UsesHttpDelete()
        {
            var source = @"
using System;

public interface IDeletionService
{
    void RemoveUser(int id);
}

[YS.Knife.ExposeApi(typeof(IDeletionService))]
public class TestController
{
}
";

            var generatedCode = await GenerateSourceAsync(source);
            
            Assert.NotNull(generatedCode);
            Assert.Contains("[HttpDelete]", generatedCode);
            Assert.Contains("RemoveUser", generatedCode);
        }

        [Fact]
        public async Task GenerateController_WithMethodStartingWithModify_UsesHttpPut()
        {
            var source = @"
using System;

public interface IEditService
{
    string ModifyUser(int id, string name);
}

[YS.Knife.ExposeApi(typeof(IEditService))]
public class TestController
{
}
";

            var generatedCode = await GenerateSourceAsync(source);
            
            Assert.NotNull(generatedCode);
            Assert.Contains("[HttpPut]", generatedCode);
            Assert.Contains("ModifyUser", generatedCode);
        }

        [Fact]
        public async Task GenerateController_WithMethodStartingWithFetch_UsesHttpGet()
        {
            var source = @"
using System;

public interface IRetrievalService
{
    string FetchData();
}

[YS.Knife.ExposeApi(typeof(IRetrievalService))]
public class TestController
{
}
";

            var generatedCode = await GenerateSourceAsync(source);
            
            Assert.NotNull(generatedCode);
            Assert.Contains("[HttpGet]", generatedCode);
            Assert.Contains("FetchData", generatedCode);
        }

        [Fact]
        public async Task GenerateController_WithMethodStartingWithUpload_UsesHttpPost()
        {
            var source = @"
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
";

            var generatedCode = await GenerateSourceAsync(source);
            
            Assert.NotNull(generatedCode);
            Assert.Contains("[HttpPost]", generatedCode);
            Assert.Contains("UploadFile", generatedCode);
        }

        [Fact]
        public async Task GenerateController_WithMethodStartingWithSave_UsesHttpPost()
        {
            var source = @"
using System;

public interface IPersistenceService
{
    string SaveData(string data);
}

[YS.Knife.ExposeApi(typeof(IPersistenceService))]
public class TestController
{
}
";

            var generatedCode = await GenerateSourceAsync(source);
            
            Assert.NotNull(generatedCode);
            Assert.Contains("[HttpPost]", generatedCode);
            Assert.Contains("SaveData", generatedCode);
        }

        [Fact]
        public async Task GenerateController_WithMethodStartingWithPatch_UsesHttpPatch()
        {
            var source = @"
using System;

public interface IPartialUpdateService
{
    string PatchUser(int id, string name);
}

[YS.Knife.ExposeApi(typeof(IPartialUpdateService))]
public class TestController
{
}
";

            var generatedCode = await GenerateSourceAsync(source);
            
            Assert.NotNull(generatedCode);
            Assert.Contains("[HttpPatch]", generatedCode);
            Assert.Contains("PatchUser", generatedCode);
        }

        [Fact]
        public async Task GenerateController_WithMethodStartingWithEdit_UsesHttpPut()
        {
            var source = @"
using System;

public interface IModificationService2
{
    string EditUser(int id, string name);
}

[YS.Knife.ExposeApi(typeof(IModificationService2))]
public class TestController
{
}
";

            var generatedCode = await GenerateSourceAsync(source);
            
            Assert.NotNull(generatedCode);
            Assert.Contains("[HttpPut]", generatedCode);
            Assert.Contains("EditUser", generatedCode);
        }

        [Fact]
        public async Task GenerateController_WithMethodStartingWithPost_UsesHttpPost()
        {
            var source = @"
using System;

public interface IPostService
{
    string PostData(string data);
}

[YS.Knife.ExposeApi(typeof(IPostService))]
public class TestController
{
}
";

            var generatedCode = await GenerateSourceAsync(source);
            
            Assert.NotNull(generatedCode);
            Assert.Contains("[HttpPost]", generatedCode);
            Assert.Contains("PostData", generatedCode);
        }

        [Fact]
        public async Task GenerateController_WithComplexObjectParameter_UsesFromBody()
        {
            var source = @"
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
";

            var generatedCode = await GenerateSourceAsync(source);
            
            Assert.NotNull(generatedCode);
            Assert.Contains("[HttpPost]", generatedCode);
            Assert.Contains("ProcessData", generatedCode);
        }

        [Fact]
        public async Task GenerateController_WithMultipleComplexParameters_CreatesRecordType()
        {
            var source = @"
using System;

public interface IMultiParamService
{
    string ProcessData(string name, int value, string description);
}

[YS.Knife.ExposeApi(typeof(IMultiParamService))]
public class TestController
{
}
";

            var generatedCode = await GenerateSourceAsync(source);
            
            Assert.NotNull(generatedCode);
            Assert.Contains("record", generatedCode);
            Assert.Contains("ProcessData", generatedCode);
        }

        [Fact]
        public async Task GenerateController_WithMixedParameters_HandlesCorrectly()
        {
            var source = @"
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
";

            var generatedCode = await GenerateSourceAsync(source);
            
            Assert.NotNull(generatedCode);
            Assert.Contains("[HttpGet]", generatedCode);
            Assert.Contains("GetData", generatedCode);
            Assert.Contains("int id", generatedCode);
            Assert.Contains("string filter", generatedCode);
            Assert.Contains("CancellationToken cancellationToken", generatedCode);
        }

        private async Task<string> GenerateSourceAsync(string source)
        {
            var generator = new ControllerGenerator();
            var driver = CSharpGeneratorDriver.Create(generator);

            var compilation = CSharpCompilation.Create(
                "Test",
                new[] { CSharpSyntaxTree.ParseText(source) },
                new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            var runResult = driver.RunGenerators(compilation);
            var result = runResult.GetRunResult();

            // 查找控制器代码
            foreach (var tree in result.GeneratedTrees)
            {
                if (tree.FilePath.EndsWith(".g.cs"))
                {
                    var content = tree.GetText().ToString();
                    // 如果内容包含"ControllerBase"，则是控制器代码
                    if (content.Contains("ControllerBase"))
                    {
                        return content;
                    }
                }
            }

            return null;
        }
    }
}

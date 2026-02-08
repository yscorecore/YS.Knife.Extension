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
    public class ControllerGeneratorTests
    {
        [Fact]
        public async Task GenerateController_WithSimpleService_GeneratesCorrectController()
        {
            var source = @"
using System;

public interface IUserService
{
    string GetUser(int id);
}

[YS.Knife.ExposeApi(typeof(IUserService))]
public class TestController
{
}
";

            var generatedCode = await GenerateSourceAsync(source);
            
            Assert.NotNull(generatedCode);
            Assert.Contains("public class UserController : ControllerBase", generatedCode);
            Assert.Contains("private readonly global::IUserService iUserService;", generatedCode);
            Assert.Contains("public UserController(global::IUserService iUserService)", generatedCode);
            Assert.Contains("public string GetUser", generatedCode);
            Assert.Contains("return this.iUserService.GetUser", generatedCode);
        }

        [Fact]
        public async Task GenerateController_WithMultipleMethods_GeneratesAllMethods()
        {
            var source = @"
using System;

public interface IProductService
{
    Product GetProduct(int id);
    void CreateProduct(Product product);
    void UpdateProduct(int id, Product product);
    void DeleteProduct(int id);
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
}

[YS.Knife.ExposeApi(typeof(IProductService))]
public class TestController
{
}
";

            var generatedCode = await GenerateSourceAsync(source);
            
            Assert.NotNull(generatedCode);
            Assert.Contains("public class ProductController : ControllerBase", generatedCode);
            Assert.Contains("[HttpGet]", generatedCode);
            Assert.Contains("[HttpPost]", generatedCode);
            Assert.Contains("[HttpPut]", generatedCode);
            Assert.Contains("[HttpDelete]", generatedCode);
            Assert.Contains("GetProduct", generatedCode);
            Assert.Contains("CreateProduct", generatedCode);
            Assert.Contains("UpdateProduct", generatedCode);
            Assert.Contains("DeleteProduct", generatedCode);
        }

        [Fact]
        public async Task GenerateController_WithCustomRoutePrefix_UsesCustomRoute()
        {
            var source = @"
using System;

public interface IOrderService
{
    Order GetOrder(int id);
}

public class Order
{
    public int Id { get; set; }
}

[YS.Knife.ExposeApi(typeof(IOrderService), RoutePrefix = ""custom/orders"")]
public class TestController
{
}
";

            var generatedCode = await GenerateSourceAsync(source);
            
            Assert.NotNull(generatedCode);
            Assert.Contains("[Route(\"custom/orders\")]", generatedCode);
        }

        [Fact]
        public async Task GenerateController_WithAllowAnonymous_AddsAllowAnonymousAttribute()
        {
            var source = @"
using System;

public interface IPublicService
{
    string GetData();
}

[YS.Knife.ExposeApi(typeof(IPublicService), AllowAnonymous = true)]
public class TestController
{
}
";

            var generatedCode = await GenerateSourceAsync(source);
            
            Assert.NotNull(generatedCode);
            Assert.Contains("[AllowAnonymous]", generatedCode);
        }

        [Fact]
        public async Task GenerateController_WithComplexParameters_GeneratesCorrectBindings()
        {
            var source = @"
using System;
using System.Threading;

public interface IComplexService
{
    string Search(string keyword, int page, int pageSize, CancellationToken cancellationToken);
}

[YS.Knife.ExposeApi(typeof(IComplexService))]
public class TestController
{
}
";

            var generatedCode = await GenerateSourceAsync(source);
            
            Assert.NotNull(generatedCode);
            Assert.Contains("string keyword", generatedCode);
            Assert.Contains("int page", generatedCode);
            Assert.Contains("int pageSize", generatedCode);
            Assert.Contains("CancellationToken cancellationToken", generatedCode);
        }

        [Fact]
        public async Task GenerateController_WithTaskReturnType_PreservesReturnType()
        {
            var source = @"
using System;
using System.Threading.Tasks;

public interface IAsyncService
{
    Task<string> GetDataAsync();
    Task SaveDataAsync(string data);
}

[YS.Knife.ExposeApi(typeof(IAsyncService))]
public class TestController
{
}
";

            var generatedCode = await GenerateSourceAsync(source);
            
            Assert.NotNull(generatedCode);
            Assert.Contains("Task<string> GetDataAsync", generatedCode);
            Assert.Contains("Task SaveDataAsync", generatedCode);
        }

        [Fact]
        public async Task GenerateController_WithMultipleBodyParameters_CreatesRecordType()
        {
            var source = @"
using System;

public interface IUpdateService
{
    void Update(int id, string name, string description);
}

[YS.Knife.ExposeApi(typeof(IUpdateService))]
public class TestController
{
}
";

            var generatedCode = await GenerateSourceAsync(source);
            
            Assert.NotNull(generatedCode);
            Assert.Contains("record", generatedCode);
            Assert.Contains("int id", generatedCode);
            Assert.Contains("string name", generatedCode);
            Assert.Contains("string description", generatedCode);
        }

        [Fact]
        public async Task GenerateController_WithServiceNameStartingWithI_RemovesPrefix()
        {
            var source = @"
using System;

public interface ICustomerService
{
    Customer GetCustomer(int id);
}

public class Customer
{
    public int Id { get; set; }
}

[YS.Knife.ExposeApi(typeof(ICustomerService))]
public class TestController
{
}
";

            var generatedCode = await GenerateSourceAsync(source);
            
            Assert.NotNull(generatedCode);
            Assert.Contains("public class CustomerController : ControllerBase", generatedCode);
            Assert.Contains("private readonly global::ICustomerService iCustomerService;", generatedCode);
        }

        [Fact]
        public async Task GenerateController_WithServiceNameEndingWithService_RemovesSuffix()
        {
            var source = @"
using System;

public interface UserService
{
    User GetUser(int id);
}

public class User
{
    public int Id { get; set; }
}

[YS.Knife.ExposeApi(typeof(UserService))]
public class TestController
{
}
";

            var generatedCode = await GenerateSourceAsync(source);
            
            Assert.NotNull(generatedCode);
            Assert.Contains("public class UserController : ControllerBase", generatedCode);
            Assert.Contains("private readonly global::UserService userService;", generatedCode);
        }

        [Fact]
        public async Task GenerateController_WithGenericMethods_SkipsGenericMethods()
        {
            var source = @"
using System;

public interface IGenericService
{
    string GetData();
    T GetItem<T>();
}

[YS.Knife.ExposeApi(typeof(IGenericService))]
public class TestController
{
}
";

            var generatedCode = await GenerateSourceAsync(source);
            
            Assert.NotNull(generatedCode);
            Assert.Contains("public string GetData()", generatedCode);
            Assert.DoesNotContain("public T GetItem<T>()", generatedCode);
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

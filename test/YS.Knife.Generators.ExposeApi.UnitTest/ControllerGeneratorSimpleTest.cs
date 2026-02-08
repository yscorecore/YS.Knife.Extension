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
    public class ControllerGeneratorSimpleTest
    {
        [Fact]
        public async Task GenerateController_SimpleTest()
        {
            var source = @"
using System;

public interface ITestService
{
    string GetTestData(int id);
}

[YS.Knife.ExposeApi(typeof(ITestService))]
public class TestController
{
}
";

            var generator = new ControllerGenerator();
            var driver = CSharpGeneratorDriver.Create(generator);

            var compilation = CSharpCompilation.Create(
                "Test",
                new[] { CSharpSyntaxTree.ParseText(source) },
                new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            var runResult = driver.RunGenerators(compilation);
            var result = runResult.GetRunResult();

            // 打印所有生成的文件路径和内容
            foreach (var tree in result.GeneratedTrees)
            {
                Console.WriteLine($"Generated file: {tree.FilePath}");
                Console.WriteLine("Content:");
                Console.WriteLine(tree.GetText().ToString());
                Console.WriteLine("------------------------");
            }

            // 查找控制器代码
            var controllerCode = string.Empty;
            foreach (var tree in result.GeneratedTrees)
            {
                if (tree.FilePath.EndsWith(".g.cs"))
                {
                    var content = tree.GetText().ToString();
                    if (content.Contains("ControllerBase"))
                    {
                        controllerCode = content;
                        break;
                    }
                }
            }

            Assert.NotNull(controllerCode);
            Assert.NotEmpty(controllerCode);
            Assert.Contains("public class TestController : ControllerBase", controllerCode);
            Assert.Contains("private readonly global::ITestService iTestService;", controllerCode);
            Assert.Contains("public TestController(global::ITestService iTestService)", controllerCode);
            Assert.Contains("public string GetTestData([FromQuery] int id)", controllerCode);
            Assert.Contains("return this.iTestService.GetTestData(id);", controllerCode);
        }
    }
}
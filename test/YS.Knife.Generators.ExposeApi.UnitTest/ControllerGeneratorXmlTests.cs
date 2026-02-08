using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using YS.Knife.Generators.ExposeApi;
using Xunit;

namespace YS.Knife.Generators.ExposeApi.UnitTest
{
    public class ControllerGeneratorXmlTests
    {
        private readonly string _testCasesPath;

        public ControllerGeneratorXmlTests()
        {
            var assemblyLocation = typeof(ControllerGeneratorXmlTests).Assembly.Location ?? string.Empty;
            var directoryName = Path.GetDirectoryName(assemblyLocation) ?? string.Empty;
            _testCasesPath = Path.Combine(directoryName, "ControllerGeneratorTestCases.xml");
        }

        [Theory]
        [InlineData("GenerateController_WithSimpleService_GeneratesCorrectController")]
        [InlineData("GenerateController_WithMultipleMethods_GeneratesAllMethods")]
        [InlineData("GenerateController_WithCustomRoutePrefix_UsesCustomRoute")]
        [InlineData("GenerateController_WithAllowAnonymous_AddsAllowAnonymousAttribute")]
        [InlineData("GenerateController_WithTaskReturnType_PreservesReturnType")]
        [InlineData("GenerateController_WithGenericMethods_SkipsGenericMethods")]
        [InlineData("GenerateController_WithServiceNameStartingWithI_RemovesPrefix")]
        [InlineData("GenerateController_WithServiceNameEndingWithService_RemovesSuffix")]
        [InlineData("GenerateController_WithCancellationToken_DoesNotBindParameter")]
        [InlineData("GenerateController_WithHttpContext_DoesNotBindParameter")]
        [InlineData("GenerateController_WithMethodStartingWithGet_UsesHttpGet")]
        [InlineData("GenerateController_WithMethodStartingWithCreate_UsesHttpPost")]
        [InlineData("GenerateController_WithMethodStartingWithUpdate_UsesHttpPut")]
        [InlineData("GenerateController_WithMethodStartingWithDelete_UsesHttpDelete")]
        [InlineData("GenerateController_WithMethodStartingWithPatch_UsesHttpPatch")]
        public async Task GenerateController_FromXml_TestCase(string caseName)
        {
            var testCase = LoadTestCase(caseName);
            var generatedCode = await GenerateSourceAsync(testCase.InputCode);

            Assert.NotNull(generatedCode);

            foreach (var expectedOutput in testCase.ExpectedOutputs)
            {
                var normalizedExpected = NormalizeWhitespace(expectedOutput.Code);
                var normalizedGenerated = NormalizeWhitespace(generatedCode);
                Assert.Contains(normalizedExpected, normalizedGenerated);
            }
        }

        private string NormalizeWhitespace(string input)
        {
            return System.Text.RegularExpressions.Regex.Replace(input, @"\s+", " ").Trim();
        }

        private TestCase LoadTestCase(string caseName)
        {
            var xml = XDocument.Load(_testCasesPath);
            var caseElement = xml.Descendants("case")
                .FirstOrDefault(c => c.Attribute("name")?.Value == caseName);

            if (caseElement == null)
            {
                throw new FileNotFoundException($"Test case '{caseName}' not found in XML file.");
            }

            var inputCode = caseElement.Element("input")?.Element("code")?.Value;
            var outputElements = caseElement.Element("output")?.Elements("code");

            var expectedOutputs = outputElements?.Select(o => new ExpectedOutput
            {
                FileName = o.Attribute("file")?.Value,
                Code = o.Value
            }).ToList() ?? new List<ExpectedOutput>();

            return new TestCase
            {
                Name = caseName,
                InputCode = inputCode,
                ExpectedOutputs = expectedOutputs
            };
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

            foreach (var tree in result.GeneratedTrees)
            {
                if (tree.FilePath.EndsWith(".g.cs"))
                {
                    var content = tree.GetText().ToString();
                    if (content.Contains("ControllerBase"))
                    {
                        return content;
                    }
                }
            }

            return string.Empty;
        }

        private class TestCase
        {
            public string Name { get; set; } = string.Empty;
            public string InputCode { get; set; } = string.Empty;
            public List<ExpectedOutput> ExpectedOutputs { get; set; } = new List<ExpectedOutput>();
        }

        private class ExpectedOutput
        {
            public string FileName { get; set; } = string.Empty;
            public string Code { get; set; } = string.Empty;
        }
    }
}

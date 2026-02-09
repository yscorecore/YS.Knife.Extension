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
        private readonly string _testDataPath;

        public ControllerGeneratorXmlTests()
        {
            var assemblyLocation = typeof(ControllerGeneratorXmlTests).Assembly.Location ?? string.Empty;
            var directoryName = Path.GetDirectoryName(assemblyLocation) ?? string.Empty;
            _testDataPath = Path.Combine(directoryName, "testdata");
        }

        [Theory]
        [InlineData("GenerateController_WithSimpleService_GeneratesCorrectController")]
        [InlineData("GenerateController_WithMultipleMethods_GeneratesAllMethods")]
        [InlineData("GenerateController_WithCustomRoutePrefix_UsesCustomRoute")]
        [InlineData("GenerateController_WithAllowAnonymous_AddsAllowAnonymousAttribute")]
        [InlineData("GenerateController_WithTaskReturnType_PreservesReturnType")]
        [InlineData("GenerateController_WithGenericMethods_SkipsGenericMethods")]
        [InlineData("GenerateController_WithServiceNameEndingWithService_RemovesSuffix")]
        [InlineData("GenerateController_WithServiceNameStartingWithI_RemovesPrefix")]
        [InlineData("GenerateController_WithCancellationToken_DoesNotBindParameter")]
        [InlineData("GenerateController_WithHttpContext_DoesNotBindParameter")]
        [InlineData("GenerateController_WithMethodStartingWithGet_UsesHttpGet")]
        [InlineData("GenerateController_WithMethodStartingWithCreate_UsesHttpPost")]
        [InlineData("GenerateController_WithMethodStartingWithUpdate_UsesHttpPut")]
        [InlineData("GenerateController_WithMethodStartingWithDelete_UsesHttpDelete")]
        [InlineData("GenerateController_WithMethodStartingWithPatch_UsesHttpPatch")]
        [InlineData("GenerateController_WithNoParameters_GeneratesMethodWithoutParameters")]
        [InlineData("GenerateController_WithVoidReturnType_GeneratesPostMethod")]
        [InlineData("GenerateController_WithIdParameter_UsesRouteParameter")]
        [InlineData("GenerateController_WithNameParameter_UsesRouteParameter")]
        [InlineData("GenerateController_WithKeyParameter_UsesRouteParameter")]
        [InlineData("GenerateController_WithMultipleRouteParameters_UsesFirstAsRoute")]
        [InlineData("GenerateController_WithMethodStartingWithQuery_UsesHttpGet")]
        [InlineData("GenerateController_WithMethodStartingWithFind_UsesHttpGet")]
        [InlineData("GenerateController_WithMethodStartingWithAdd_UsesHttpPost")]
        [InlineData("GenerateController_WithMethodStartingWithRemove_UsesHttpDelete")]
        [InlineData("GenerateController_WithMethodStartingWithModify_UsesHttpPut")]
        [InlineData("GenerateController_WithMethodStartingWithFetch_UsesHttpGet")]
        [InlineData("GenerateController_WithMethodStartingWithUpload_UsesHttpPost")]
        [InlineData("GenerateController_WithMethodStartingWithSave_UsesHttpPost")]
        [InlineData("GenerateController_WithMethodStartingWithEdit_UsesHttpPut")]
        [InlineData("GenerateController_WithMethodStartingWithPost_UsesHttpPost")]
        [InlineData("GenerateController_WithComplexObjectParameter_UsesFromBody")]
        [InlineData("GenerateController_WithMultipleComplexParameters_CreatesRecordType")]
        [InlineData("GenerateController_WithMixedParameters_HandlesCorrectly")]
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
            var xmlFilePath = Path.Combine(_testDataPath, $"{caseName}.xml");
            
            if (!File.Exists(xmlFilePath))
            {
                throw new FileNotFoundException($"Test case file '{xmlFilePath}' not found.");
            }

            var xml = XDocument.Load(xmlFilePath);
            var caseElement = xml.Element("case");

            if (caseElement == null)
            {
                throw new InvalidOperationException($"Invalid XML format in '{xmlFilePath}'. Root element 'case' not found.");
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

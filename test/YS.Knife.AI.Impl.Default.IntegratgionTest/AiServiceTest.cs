using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using YS.Knife.AI.Core;
using YS.Knife.Hosting;

namespace YS.Knife.AI.Impl.Default.IntegratgionTest
{
    [AutoConstructor]
    public partial class AiServiceTest : YS.Knife.Hosting.KnifeHost
    {
        private readonly ITestOutputHelper outputHelper;
        [Fact]
        public void ShouldGetAiService()
        {
            var aiService = this.GetService<IAiService>();
            aiService.Should().NotBeNull();
        }
        //[InjectConfiguration("OpenAI:Ali:ApiKey")]
        //private const string AliApiKey = "xxxxx";
        [InjectConfiguration("OpenAI:Ali:BaseUrl")]
        private const string AliBaseUrl = "https://dashscope.aliyuncs.com/compatible-mode/v1";
        [InjectConfiguration("OpenAI:Ali:UploadFilePurpose")]
        private const string AliUploadFilePurpose = "file-extract";
        //[InjectConfiguration("OpenAI:Zijie:ApiKey")]
        //private const string ZJApiKey = "xxxx";
        [InjectConfiguration("OpenAI:Zijie:BaseUrl")]
        private const string ZjBaseUrl = "https://ark.cn-beijing.volces.com/api/v3";


        //[InjectConfiguration("OpenAI:Baidu:ApiKey")]
        //private const string BaiduApiKey = "xxxx";
        [InjectConfiguration("OpenAI:Baidu:BaseUrl")]
        private const string BaiduBaseUrl = "https://qianfan.baidubce.com/v2";
        //[Fact]
        //public async Task ShouldReturnJsonObject()
        //{
        //    var aiService = this.GetService<IAiService>();
        //    var data = AiInputData.From("Images/3.jpg");
        //    var models = new[] { "openai::Zijie/doubao-seed-2-0-lite-260428", "openai::Ali/qwen3.7-plus", "openai::Baidu/ernie-5.0" };
        //    var model = models.RandomOne();
        //    var stopwatch = new Stopwatch();
        //    stopwatch.Start();
        //    var result = await aiService.RecognizeImageAsObject<AiResult>(data.AsArray(), model, "图片中是否存在【抽烟】的违规异常");
        //    stopwatch.Stop();
        //    outputHelper.WriteLine("Model: {0}, Time taken: {1}", model, stopwatch.Elapsed);
        //    result.Description.Should().NotBeNull();
        //}
        //[Fact]
        //public async Task ShouldReturnJsonArray()
        //{
        //    var aiService = this.GetService<IAiService>();
        //    var data = AiInputData.From("Images/1.jpg");
        //    var models = new[] { "openai::Zijie/doubao-seed-2-0-lite-260428", "openai::Baidu/ernie-5.0" };
        //    var model = models.RandomOne();
        //    var stopwatch = new Stopwatch();
        //    stopwatch.Start();
        //    var result = await aiService.RecognizeImageAsArray<AiResult2>(data.AsArray(), model, "图中是礼薄随礼的信息，你需要识别随礼人的名称和金额，请整理成结构化的数据");
        //    stopwatch.Stop();
        //    outputHelper.WriteLine("Model: {0}, Time taken: {1}", model, stopwatch.Elapsed);
        //    result.Length.Should().Be(10);
        //}
        //[Fact]
        //public async Task ShouldReturnJsonArrayWhenInputExcel()
        //{
        //    var aiService = this.GetService<IAiService>();
        //    var data = AiInputData.From("Images/菜品管理.xlsx");
        //    var models = new[] { "openai::Ali/qwen-long" };
        //    var model = "openai::Ali/qwen-long";
        //    var stopwatch = new Stopwatch();
        //    stopwatch.Start();

        //    var result = await aiService.RecognizeDocumentAsArray<AiResult3>(data.AsArray(), model, "请将文档中的内容识别为结构化的信息,包含菜品名称和菜品分类等信息");
        //    stopwatch.Stop();
        //    outputHelper.WriteLine("Model: {0}, Time taken: {1}", model, stopwatch.Elapsed);
        //    result.Length.Should().Be(119);
        //}
        private class AiResult
        {
            [Description("是否有异常")]
            public bool HasException { get; set; }
            [Description("异常的描述")]
            public string Description { get; set; }
        }
        private record AiResult2
        {
            [Description("随礼金额")]
            public decimal Money { get; set; }

            [Description("随礼人")]
            public string UserName { get; set; }
        }
        private record AiResult3
        {
            [Description("菜品名称")]
            public string Name { get; set; }
            [Description("菜品分类")]
            public string Category { get; set; }
            [Description("菜品类型")]
            public string Kind { get; set; }
            [Description("食材名称")]
            public string MaterialName { get; set; }

            [Description("人均带量")]
            public decimal Amount { get; set; }
            [Description("适用餐次")]
            public string[] MealKinds { get; set; }
            [Description("适用季节")]
            public string[] Seasons { get; set; }
        }
    }
}

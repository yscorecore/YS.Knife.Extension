using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using YS.Knife.AI.Core;
using YS.Knife.Hosting;

namespace YS.Knife.AI.Impl.Default.IntegratgionTest
{
    public class AiServiceTest : YS.Knife.Hosting.KnifeHost
    {
        [Fact]
        public void ShouldGetAiService()
        {
            var aiService = this.GetService<IAiService>();
            aiService.Should().NotBeNull();
        }
        [InjectConfiguration("OpenAI:ApiKey")]
        private string ApiKey = "sk-cb7a855720ad4f1ea46642ea5b7681ad";
        [InjectConfiguration("OpenAI:BaseUrl")]
        private string BaseUrl = "https://dashscope.aliyuncs.com/compatible-mode/v1";

        [Fact]
        public async Task ShouldReturnJsonObject()
        {
            var aiService = this.GetService<IAiService>();
            var data = AiInputData.From("Images/3.jpg");
            var result = await aiService.RecognizeImageAsObject<AiResult>(data.AsArray(), "openai", "qwen-vl-plus", "图片中是否存在【抽烟】的违规异常");
            result.HasException.Should().BeFalse();
        }
        [Fact]
        public async Task ShouldReturnJsonArray()
        {
            var aiService = this.GetService<IAiService>();
            var data = AiInputData.From("Images/1.jpg");
            var result = await aiService.RecognizeImageAsArray<AiResult2>(data.AsArray(), "openai", "Qwen3.7-Plus", "图中是礼薄随礼的信息，请整理成结构化的数据");
            result.Length.Should().Be(10);
        }
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
    }
}

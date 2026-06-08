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
    public class UnitTest1:YS.Knife.Hosting.KnifeHost
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
        public async Task Test2()
        {
            var aiService = this.GetService<IAiService>();
            
            // 尝试多个可能的路径来查找图片文件
            string[] possiblePaths = new[]
            {
                // 当前工作目录
                Path.Combine(Directory.GetCurrentDirectory(), "Images", "1.jpg"),
                // 程序集所在目录
                Path.Combine(Path.GetDirectoryName(typeof(UnitTest1).Assembly.Location) ?? "", "Images", "1.jpg"),
                // AppContext 基目录
                Path.Combine(AppContext.BaseDirectory, "Images", "1.jpg"),
                // 项目源码目录（测试项目目录）
                Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "test", "YS.Knife.AI.Impl.Default.IntegratgionTest", "Images", "1.jpg"),
                // 解决方案根目录下的测试项目
                Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "test", "YS.Knife.AI.Impl.Default.IntegratgionTest", "Images", "1.jpg"),
                // 绝对路径 - 从源码目录开始
                @"c:\Users\Lenovo\source\repos\YS.Knife.Extension\test\YS.Knife.AI.Impl.Default.IntegratgionTest\Images\1.jpg"
            };
            
            string imagePath = possiblePaths.FirstOrDefault(p => !string.IsNullOrEmpty(p) && File.Exists(p));
            
            if (string.IsNullOrEmpty(imagePath))
            {
                throw new FileNotFoundException("图片文件未找到。尝试了以下路径：\n" + string.Join("\n", possiblePaths));
            }
            
            var imageBytes = File.ReadAllBytes(imagePath);
            var base64String = Convert.ToBase64String(imageBytes);
            var result = await aiService.RecognizeImageAsObject<AiResult>(base64String, "openai", "qwen-vl-plus", "图片中是否存在【未按规定佩戴帽子】的违规异常");
            result.HasException.Should().BeFalse();
        }

        public class AiResult
        {
            [Description("是否有异常")]
            public bool HasException { get; set; }
            [Description("异常的描述")]
            public string Description { get; set; }
        }
    }
}
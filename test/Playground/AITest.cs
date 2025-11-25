using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenAI;
using OpenAI.Chat;
using Xunit.Abstractions;
using static Playground.DeepSeekHttpClient;

namespace Playground
{
    [AutoConstructor]
    public partial class AITest
    {
        private ITestOutputHelper helper;
        [Fact]
        public async Task TestAI()
        {
            //var clientOptions = new OpenAI.OpenAIClientOptions();
            //clientOptions.Endpoint = new Uri("https://api.deepseek.com/v1");
            //var apiKey = new ApiKeyCredential("sk-bf44288e5b14471bae1d1ce7044d066b");
            //var client = new OpenAI.OpenAIClient(apiKey, clientOptions);
            //var chatClient = client.GetChatClient("deepseek-chat").AsIChatClient();
            //var res = await chatClient.GetResponseAsync("你是谁？");
            //helper.WriteLine(res.ToString());

        }

        [Fact]
        public async Task TestMenu()
        {
            var deepseekClient = new DeepSeekHttpClient("sk-bf44288e5b14471bae1d1ce7044d066b");
            //var clientOptions = new OpenAI.OpenAIClientOptions();
            //clientOptions.Endpoint = new Uri("https://api.deepseek.com/v1");
            //var apiKey = new ApiKeyCredential("sk-bf44288e5b14471bae1d1ce7044d066b");
            //var client = new OpenAI.OpenAIClient(apiKey, clientOptions);
            //var chatClient = client.GetChatClient("deepseek-reasoner");
            // var file = @"C:\Users\Lenovo\Downloads\执行结果2.csv";
            var file = @"C:\Users\Lenovo\Downloads\执行结果1.csv";
            var allMenuInfos = File.ReadAllText(file);

            var season = "冬";
            var minAmount = 4;
            var maxAmount = 6;
            var startDate = new DateTime(2025, 11, 1);
            var endDate = new DateTime(2025, 11, 6);
            var mainFoodType = "2025-11-01:面食,2025-11-02:不限制,2025-11-03:面食,2025-11-04:米饭,2025-11-05:面食,2025-11-06:米饭";
            var res = await deepseekClient.GetCompletionAsync(new DeepSeekMessage[] {
     DeepSeekMessage.CreateSystemMessage("你是学校的校园餐管理员,在制作营养食谱有很丰富的经验"),
     DeepSeekMessage.CreateUserMessage(@$"
学校可选的菜品信息如下，csv格式:

{allMenuInfos}

"),
     DeepSeekMessage.CreateUserMessage($@"
食谱制作要求如下：
1. 要求生成{season}季的{"午餐"}食谱，以上的csv文件中包含了所有可选的菜品信息,包含了适用季节和适用餐次
2. 食谱的开始日期{startDate:yyyy-MM-dd}，结束日期{endDate:yyyy-MM-dd}，中间不间断（不考虑节假日）
3. 每天的价格在{minAmount}元到{maxAmount}元之间（必须达到），菜品的价格在以上的csv中有体现，每天的价格是当天所有菜品的价格之和，这个要求必须达到
4. 食谱需要荤素搭配，营养均衡，并且每餐都需要有主食,每餐都必须有荤菜和素菜，最少一个菜品，最多6个菜品。每天必须有主食， 如果主食是面食，则菜品可以相应少些或者没有
5. 主食要求如下：{mainFoodType}
"),
    DeepSeekMessage.CreateUserMessage(@"

输出的要求如下：
1. 要求生成一个数组，数组的每一项代表每一天的食谱，格式如下：
[
  {
    ""date:""2025-11-01"",
    ""totalprice"":1.6076,
    ""dishes"": [
            {""id"":""9266c3cc-fb6f-4112-b21e-7d96d2a7ba41"", ""name"":""米饭"", ""price"":0.5676},
            {""id"":""30389547-8ee0-4082-8a1d-4764bc9d7e31"", ""name"":""青瓜炒猪肉"", ""price"":1.0400}
        ]
 }
]
其中totalprice 代表当天的菜品的价格总和

2. 如果能生成符合要求的食谱，则输出格式为json字符串(需要加换行符，方便阅读)
3. 如果不能生成符合要求的食谱，则直接给出不能输出的原因，不要json格式，只要返回文本就可以，例如可选的菜品过少。
4. 不需要思考过程，直接给出结果
5. 返回结果只有json，不要```json标记
") });
 
            helper.WriteLine(res.ToString());

        }
    }
}

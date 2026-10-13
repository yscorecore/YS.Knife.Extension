using Xunit.Abstractions;
using YS.Knife.Hosting;

namespace YS.Knife.Sms.Impl.Submail.IntegrationTest
{

    public class SubmailSmsServiceTest : YS.Knife.Hosting.KnifeHost
    {
        private ITestOutputHelper output;
        // test
        [InjectConfiguration("Submail:AppId")]
        private const string AppId = "95331";
        [InjectConfiguration("Submail:AppKey")]
        private const string AppKey = "xxxxxxxx";


        //[Fact]
        public async Task ShouldSendSmsMessage()
        {
            var service = this.GetService<ISmsService>();
            await service.SendSms("136xxxxxxxx", "9vvZv", new Dictionary<string, object>
            {
                ["date"] = "2023年5月15日",
                ["url_link"] = "https://wxaurl.cn/4rCPJBYqsCl",
                ["exception_count"] = 100
            });

        }
    }
}

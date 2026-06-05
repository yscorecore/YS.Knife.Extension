using System.IO;
using System.Threading.Tasks;

namespace YS.Knife.Wechat.MiniProgram.IntegrationTest
{
    public class WechatClientTest : YS.Knife.Hosting.KnifeHost
    {
        private const string TestWechatAppId = "wx979921ee95c07b6c";
        private const string TestWechatAppSecret = "84237871eb3a0ff7fed34114275f884e";


        [Fact]
        public void ShouldGetWechatClientInstance()
        {
            this.GetService<WechatClient>().Should().NotBeNull();
        }
        [Fact]
        public async Task ShouldGetWechatAccessToken()
        {
            var client = this.GetService<WechatClient>();
            var token = await client.GetAccessToken(TestWechatAppId, TestWechatAppSecret);
            token.AccessToken.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task ShouldThrowErrorWhenGetUnlimitedQrCodeWithInvalidAccesstoken()
        {
            var client = this.GetService<WechatClient>();
            Func<Task<Stream>> act = () => client.GetUnlimited("abc", new GetWxacodeunlimitRequest
            {
                Scene = "abc",
                Page = "abc",
                CheckPath = false
            });
            await act.Should().ThrowAsync<WechatException>();
        }
        [Fact]
        public async Task ShouldGetUnlimitedQrCode()
        {
            var client = this.GetService<WechatClient>();
            var token = await client.GetAccessToken(TestWechatAppId, TestWechatAppSecret);
            var imgStream = await client.GetUnlimited(token.AccessToken, new GetWxacodeunlimitRequest
            {
                Scene = "abc",
                Page = "abc",
                CheckPath = false
            });
            var buffer = new byte[imgStream.Length];
            imgStream.Read(buffer, 0, buffer.Length);
            buffer.Length.Should().BeGreaterThan(200);
        }
    }
}

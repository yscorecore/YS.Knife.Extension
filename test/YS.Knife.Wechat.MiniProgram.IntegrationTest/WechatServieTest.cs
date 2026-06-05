using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YS.Knife.Hosting;

namespace YS.Knife.Wechat.MiniProgram.IntegrationTest
{
    public class WechatServieTest : YS.Knife.Hosting.KnifeHost
    {
        [InjectConfiguration("Wechat:AppId")]
        private const string TestWechatAppId = "wx979921ee95c07b6c";
        [InjectConfiguration("Wechat:AppSecret")]
        private const string TestWechatAppSecret = "84237871eb3a0ff7fed34114275f884e";

        [Fact]
        public async Task ShouldGetUnlimitedQrCode()
        {
            var service = this.GetService<IWechatService>();
            var imgStream = await service.GetUnlimited("abc");
            var buffer = new byte[imgStream.Length];
            imgStream.Read(buffer, 0, buffer.Length);
            buffer.Length.Should().BeGreaterThan(200);
        }
    }
}

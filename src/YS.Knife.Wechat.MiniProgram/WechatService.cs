using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace YS.Knife.Wechat.MiniProgram
{
    [Service]
    [AutoConstructor]
    public partial class WechatService : IWechatService
    {
        private readonly WechatClient client;
        private readonly WechatOptions options;
        private readonly WechatTokenManager tokenManager;
        public Task<Code2SessionResponse> Code2Session(string jscode)
        {
            return client.Code2Session(options.AppId, options.AppSecret, jscode);
        }

        public async Task<GenerateUrlLinkResponse> GenerateUrlLink(GenerateUrlLinkRequest request)
        {
            var accessToken = await tokenManager.GetAccessToken();
            return await client.GenerateUrlLink(accessToken, request with { EnvVersion = options.Env });
        }
        public async Task<WechatResponse> NotificationMessage<T>(WechatNotificationMessage<T> body)
        {
            var accessToken = await tokenManager.GetAccessToken();
            return await client.NotificationMessage(accessToken, body);
        }
        public async Task<Stream> GetUnlimited(string scene, string page = null, bool checkPage = false, bool isHyaline = false)
        {
            var accessToken = await tokenManager.GetAccessToken();
            return await client.GetUnlimited(accessToken, new GetWxacodeunlimitRequest
            {
                Scene = scene,
                Page = page,
                CheckPath = checkPage,
                IsHyaline = isHyaline,
                EnvVersion = options.Env,
            });
        }
    }
}

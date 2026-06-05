using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YS.Knife.TokenMan;

namespace YS.Knife.Wechat.MiniProgram
{
    [Service(typeof(WechatTokenManager))]
    [AutoConstructor]
    public partial class WechatTokenManager
    {
        private readonly WechatClient wechatClient;
        private readonly WechatOptions options;
        private readonly ITokenManager tokenManager;
        public async Task<string> GetAccessToken()
        {
            var token = await tokenManager.GetToken<GetAccessTokenResponse>(
                $"AccssToken:{options.AppId}",
                (c) => wechatClient.GetAccessToken(options.AppId, options.AppSecret),
                t => TimeSpan.FromSeconds(t.ExpiresIn), bufferSeconds: 120, preloadSeconds: 30);
            return token.AccessToken;
        }
    }
}

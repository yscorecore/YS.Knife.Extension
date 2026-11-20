using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace YS.Knife.Wechat.MiniProgram
{
    [Service(typeof(WechatClient))]
    [AutoConstructor]
    public partial class WechatClient
    {
        private readonly HttpClient httpClient;
        private const string BaseUrl = "https://api.weixin.qq.com";
        private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions()
        {

        };

        private async Task<T> GetAsObject<T>(string path, object param = default)
            where T : WechatResponse
        {
            return await httpClient.GetAsObject<T>(BaseUrl, path, param: param, jsonOptions: JsonSerializerOptions, checkStatusCode: true, businessExceptionFactory: CheckResponse);
        }
        private async Task<T> PostAsObject<T>(string path, object param = default, object body = default)
           where T : WechatResponse
        {
            return await httpClient.PostAsObject<T>(BaseUrl, path, param: param, body: body, jsonOptions: JsonSerializerOptions, checkStatusCode: true, businessExceptionFactory: CheckResponse);
        }
        private Exception CheckResponse<T>(T response, string content)
            where T : WechatResponse
        {
            if (!response.IsSuccess)
            {
                throw new WechatException(response.ErrorCode, response.ErrorMessage);
            }
            return null;
        }

        public Task<Code2SessionResponse> Code2Session(string appId, string appSecret, string jscode)
        {
            return this.GetAsObject<Code2SessionResponse>("sns/jscode2session",
                 param: new
                 {
                     appid = appId,
                     secret = appSecret,
                     js_code = jscode,
                     grant_type = "authorization_code"
                 });
        }

        public Task<GetAccessTokenResponse> GetAccessToken(string appId, string appSecret)
        {
            return this.GetAsObject<GetAccessTokenResponse>("cgi-bin/token", new
            {
                appid = appId,
                secret = appSecret,
                grant_type = "client_credential"
            });
        }

        public async Task<Stream> GetUnlimited(string accessToken, GetWxacodeunlimitRequest req)
        {
            var response = await httpClient.SendAsResponse(HttpMethod.Post, BaseUrl, "/wxa/getwxacodeunlimit", param: new { access_token = accessToken }, body: req, jsonOptions: JsonSerializerOptions);
            if (response.Content.Headers.ContentType.MediaType.Contains("json"))
            {
                var content = await response.Content.ReadAsStringAsync();
                var wechatResponse = content.AsJsonObject<WechatResponse>(JsonSerializerOptions);
                var err = CheckResponse(wechatResponse, content);
                if (err != null)
                {
                    throw err;
                }
            }
            return await response.Content.ReadAsStreamAsync();

        }

        public Task<WechatResponse> NotificationMessage<T>(string accessToken, WechatNotificationMessage<T> body)
        {
            return this.PostAsObject<WechatResponse>("/cgi-bin/message/subscribe/send", new { access_token = accessToken }, body);
        }
        public Task<GenerateUrlLinkResponse> GenerateUrlLink(string accessToken, GenerateUrlLinkRequest request)
        {
            return this.PostAsObject<GenerateUrlLinkResponse>("/wxa/generate_urllink", new { access_token = accessToken }, request);
        }

    }
}

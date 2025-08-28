using System.Net;
using Microsoft.Extensions.Caching.Distributed;
using YS.Knife.Lock;

namespace YS.Knife.Tts.Impl.Aliyun
{
    [Service(typeof(TokenManager))]
    [AutoConstructor]
    public partial class TokenManager
    {
        private readonly HttpClient httpClient;
        private readonly IDistributedCache cache;
        private readonly AliyunTtsOptions options;
        private readonly ILockService lockService;

        public async Task<string> GetToken()
        {
            var key = $"AliyunTtsToken:{options.AccessKeyId}";
            var token = await cache.GetStringAsync(key);
            if (string.IsNullOrEmpty(token))
            {
                //需要加lock
                await lockService.RunWithLock("lock:" + key, async () =>
                {
                    var (newToken, expireTime) = await GetTokenInternal();
                    await cache.SetStringAsync(key, newToken, new DistributedCacheEntryOptions
                    {
                        AbsoluteExpiration = expireTime.AddMinutes(-1),
                    });
                });
                return await cache.GetStringAsync(key);
            }
            else
            {
                return token;
            }
        }
        private async Task<(string, DateTimeOffset)> GetTokenInternal()
        {
            var data = new Dictionary<string, string>
            {
                { "AccessKeyId", options.AccessKeyId },
                { "Action", "CreateToken" },
                { "Version", "2019-02-28" },
                { "Format", "JSON" },
                { "RegionId", options.RegionId },
                { "SignatureMethod", "HMAC-SHA1" },
                { "SignatureVersion", "1.0" },
                { "SignatureNonce", Guid.NewGuid().ToString() },
                { "Timestamp", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ") }
            };
            // data["Signature"] = AliyunSignature.CalcSignature("GET", "/", data, options.AccessKeySecret);
            // var formUrlEncodeContent = new FormUrlEncodedContent(data);
            var response = await httpClient.GetAsObject<TokenResponse>(
                baseUrl: options.TokenUrl,
                path: "/".CombinQueryString(AliyunSignature.BuildQueryStringWithSignature("GET", "/", data, options.AccessKeySecret)),
                header: null,
                param: null);
            if (string.IsNullOrEmpty(response.ErrMsg))
            {
                return (response.Token.Id, DateTimeOffset.FromUnixTimeSeconds(response.Token.ExpireTime));
            }
            else
            {
                throw new Exception($"Get aliyun token failed. {response.ErrMsg}");
            }
        }


    }

    public record class TokenResponse
    {
        public string NlsRequestId { get; set; }
        public string RequestId { get; set; }
        public string ErrMsg { get; set; }
        public TokenInfo Token { get; set; }
    }
    public record class TokenInfo
    {
        public long ExpireTime { get; set; }
        public string Id { get; set; }
        public string UserId { get; set; }
    }
}

using System.Net.Http.Json;

namespace YS.Knife.Tts.Impl.Aliyun
{
    [Service]
    [AutoConstructor]
    public partial class TextSpeechService : ITextSpeechService
    {
        private readonly AliyunTtsOptions options;
        private readonly TokenManager tokenManager;
        private readonly HttpClient httpClient;
        public async Task<TextSpeechRes> TextToSpeech(TextSpeechReq req, CancellationToken cancellationToken)
        {
            /**
             * 
 {
    "appkey":"31f932fb",
    "text":"今天是周一，天气挺好的。",
    "token":"450343c793aaaaaa****",
    "format":"wav"
  }
             * 
             **/
            var token = await tokenManager.GetToken();
            var response = await httpClient.SendAsResponse(HttpMethod.Post, options.BaseUrl, "/stream/v1/tts", null, null,
                new
                {
                    appkey = options.AppKey,
                    text = req.Text ?? string.Empty,
                    token = token,
                    format = "mp3"
                }, null);
            if (response.IsSuccessStatusCode && response.Content.Headers.ContentType?.MediaType == "audio/mpeg")
            {
                var audio = await response.Content.ReadAsByteArrayAsync(cancellationToken);
                return new TextSpeechRes { Audio = audio };
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new Exception($"Aliyun tts error. status code {response.StatusCode} raw text: {error}");
            }
        }

    }
}

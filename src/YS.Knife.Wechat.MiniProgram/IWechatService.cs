
namespace YS.Knife.Wechat.MiniProgram
{
    public interface IWechatService
    {
        Task<Code2SessionResponse> Code2Session(string jscode);
        Task<Stream> GetUnlimited(string scene, string page = null, bool checkPage = false, bool isHyaline = false);
        Task<GenerateUrlLinkResponse> GenerateUrlLink(GenerateUrlLinkRequest request);
        Task<WechatResponse> NotificationMessage<T>(WechatNotificationMessage<T> body);
    }

}

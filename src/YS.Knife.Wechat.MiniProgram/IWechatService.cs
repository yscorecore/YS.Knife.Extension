
namespace YS.Knife.Wechat.MiniProgram
{
    public interface IWechatService
    {
        Task<Code2SessionResponse> Code2Session(string jscode);
        Task<Stream> GetUnlimited(string scene, string page = null, bool checkPage = false, bool isHyaline = false);
    }
}

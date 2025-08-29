using Microsoft.Extensions.DependencyInjection;

namespace YS.Knife.Sms.Impl.Submail
{
    [AutoConstructor]
    [Service(Lifetime = ServiceLifetime.Singleton)]
    public partial class SmsService : ISmsService
    {
        private readonly HttpClient httpClient;
        private readonly SubmailOptions submailOptions;
        public async Task SendSms(SmsInfo smsInfo)
        {
            var res = await httpClient.PostAsObject<SubMailSendSmsResponse>("https://api-v4.mysubmail.com", "sms/xsend", header: null,
                body: new
                {
                    to = smsInfo.Phone,
                    appid = submailOptions.AppId,
                    project = smsInfo.Template,
                    vars = smsInfo.Args.ToJsonText(),
                    signature = submailOptions.AppKey,
                });
            res.EnsureSuccessCode();
        }
    }
}

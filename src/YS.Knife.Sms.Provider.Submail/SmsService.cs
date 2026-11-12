namespace YS.Knife.Sms.Provider.Submail
{
    [AutoConstructor]
    [Service]
    public partial class SmsService : ISmsProvider
    {
        private readonly HttpClient httpClient;
        private readonly SubmailOptions submailOptions;

        public string Name => "submail";

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

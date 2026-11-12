namespace YS.Knife.Sms
{
    public interface ISmsService
    {
        Task SendTemplate(string phone, string template, Dictionary<string, object> args, CancellationToken cancellationToken);
    }

    public interface ISmsProvider
    {
        string Name { get; }
        Task SendSms(SmsInfo smsInfo);
    }
    public static class SmsServiceExtensions
    {
        public static Task SendSms(this ISmsProvider smsService, string phone, string template, Dictionary<string, object> args)
        {
            return smsService.SendSms(new SmsInfo { Phone = phone, Template = template, Args = args });
        }
    }
}

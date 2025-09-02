using System.Reflection;

namespace YS.Knife.Sms
{
    public interface ISmsService
    {
        Task SendSms(SmsInfo smsInfo);
    }
    public static class SmsServiceExtensions
    {
        public static Task SendSms(this ISmsService smsService, string phone, string template, IDictionary<string, object> args)
        {
            return smsService.SendSms(new SmsInfo { Phone = phone, Template = template, Args = args });
        }
    }
}

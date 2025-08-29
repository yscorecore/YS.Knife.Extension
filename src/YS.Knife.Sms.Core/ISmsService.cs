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
        public static Task SendSms<T>(this ISmsService smsService, string phone, string template, T args)
        {
            var dic = new Dictionary<string, object>();
            typeof(T).GetProperties().ToList().ForEach(p =>
            {
                dic[p.Name.ToLowerInvariant()] = p.GetValue(args);
            });
            return smsService.SendSms(new SmsInfo { Phone = phone, Template = template, Args = dic });
        }
    }
}

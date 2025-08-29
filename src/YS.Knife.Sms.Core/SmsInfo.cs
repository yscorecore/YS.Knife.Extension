namespace YS.Knife.Sms
{
    public record SmsInfo
    {
        public string Phone { get; set; }
        public string Template { get; set; }
        public IDictionary<string, object> Args { get; set; }
    }
}

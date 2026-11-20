namespace YS.Knife.Sms
{
    public record SmsInfo
    {

        public string Phone { get; set; }
        public string Template { get; set; }
        public Dictionary<string, object> Args { get; set; }
    }
}

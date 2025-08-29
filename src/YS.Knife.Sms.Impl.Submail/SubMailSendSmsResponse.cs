using System.Text.Json.Serialization;

namespace YS.Knife.Sms.Impl.Submail
{
    public record SubMailSendSmsResponse : SubmailBaseResponse
    {

        [JsonPropertyName("send_id")]
        public string SendId { get; set; }

        public int Fee { get; set; }

    }
}

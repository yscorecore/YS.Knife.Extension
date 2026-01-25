using System.Text;
using System.Text.Json;

namespace YS.Knife.WebHooks
{
    public record WebhookConfig
    {
        public string CallbackUrl { get; set; } = null!;
        public string SecretKey { get; set; } = null!;
        public bool EventDataOnly { get; set; } = false;
        public Encoding? Encoding { get; set; }
        public JsonSerializerOptions? JsonOptions { get; set; }
    }
}

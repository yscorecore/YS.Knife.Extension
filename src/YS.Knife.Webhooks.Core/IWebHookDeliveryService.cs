using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YS.Knife.WebHooks
{
    public interface IWebHookDeliveryService
    {
        Task SendEvent<T>
            (WebhookEvent<T> webhookEvent, WebhookConfig config, CancellationToken token = default);
    }
}

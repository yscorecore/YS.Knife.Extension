namespace YS.Knife.Sms.Impl.Default
{
    [AutoConstructor]
    [Service]
    public partial class DefaultSmsService : ISmsService
    {
        private readonly IEnumerable<ISmsProvider> _smsProviders;
        public Task SendTemplate(string phone, string template, Dictionary<string, object> args, CancellationToken cancellationToken)
        {
            var (templateProvider, templateName) = ParseTemplate(template);
            var providers = _smsProviders.ToList();
            if (string.IsNullOrWhiteSpace(templateProvider))
            {
                if (providers.Count == 1)
                {
                    var provider = providers[0];
                    return provider.SendSms(new SmsInfo { Phone = phone, Template = templateName, Args = args });
                }
                else if (providers.Count == 0)
                {
                    throw new InvalidOperationException("No SMS provider is registered.");
                }
                else
                {
                    throw new InvalidOperationException("Multiple SMS providers are registered. Please specify a provider in the template.");
                }
            }
            else
            {
                var provider = providers.FirstOrDefault(p => string.Equals(p.Name, templateProvider, StringComparison.OrdinalIgnoreCase))
                    ?? throw new Exception($"SMS provider '{templateProvider}' not found.");
                return provider.SendSms(new SmsInfo { Phone = phone, Template = templateName, Args = args });
            }
        }
        (string? provider, string template) ParseTemplate(string template)
        {
            var parts = template.Split('@');
            if (parts.Length == 2)
            {
                return (parts[1], parts[0]);

            }
            else if (parts.Length == 1)
            {
                return (null, parts[0]);
            }
            else
            {
                throw new ArgumentException($"Invalid template format: {template}. Expected format: 'template@provider'");
            }

        }
    }
}

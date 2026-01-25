namespace YS.Knife.WebHooks
{
    public record WebhookEvent<T>(string EventId,
        string EventType,
        DateTimeOffset EventTime,
        T Data)
    {

    }
}

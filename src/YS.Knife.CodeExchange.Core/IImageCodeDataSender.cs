namespace YS.Knife.CodeExchange
{
    public interface IImageCodeDataSender
    {
        Task<bool> SendData(string sence, object data, CancellationToken cancellationToken);
        Task<SenceInfo> QuerySenceInfo(string sence, CancellationToken cancellationToken);
        public record SenceInfo(string Name, object Arugments, DateTimeOffset Expired);
    }
}

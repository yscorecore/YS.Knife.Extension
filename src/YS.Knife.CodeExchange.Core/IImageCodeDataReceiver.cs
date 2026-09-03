namespace YS.Knife.CodeExchange
{
    public interface IImageCodeDataReceiver
    {
        Task<ImageCodeInfo> CreateImageCode(string name, object args, CancellationToken cancellationToken);
        //可以考虑使用SingalR来实现实时推送数据到客户端，减少轮询请求
        Task<ImageCodeRequest> QueryData(Guid id, CancellationToken cancellationToken);
        Task<bool> Release(Guid id, CancellationToken cancellationToken);
        public record ImageCodeInfo(Guid Id, DateTimeOffset Exipred, byte[] ImageBytes);
        public record ImageCodeRequest(bool IsValid, object? Data);
    }
}

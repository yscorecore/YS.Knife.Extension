using YS.Knife.Service;

namespace YS.Knife.ActiveKey.Core
{
    public interface IActiveKeyService
    {
        Task<string> Request(string owner, Dictionary<string, object> requestContext, CancellationToken cancellationToken);
        Task Consume(string code, Dictionary<string, object> consumeContext, CancellationToken cancellationToken);
    }
    public enum ActiveKeyStatue
    {
        New = 0,
        Requested = 1,
        Used = 2,
    }
    public record ActiveKeyInfo<T> : BaseDto<T>
    {
        public string Code { get; set; } = null!;
        public Dictionary<string, object> Meta { get; set; } = new();
        public DateTime? LatestRequestTime { get; set; }
        public Dictionary<string, object> RequestContext { get; set; } = new();
        public DateTime? LatestConsumeTime { get; set; }
        public Dictionary<string, object> ConsumeContext { get; set; } = new();

        public ActiveKeyStatue Status { get; set; }

    }
}

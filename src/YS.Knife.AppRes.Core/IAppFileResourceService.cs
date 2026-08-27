using YS.Knife.Query;
using YS.Knife.Service;

namespace YS.Knife.AppRes
{
    public interface IAppFileResourceService
    {
        Task<PagedList<AppGroupFileResourceInfo>> Query(string group, LimitQueryInfo req, CancellationToken cancellationToken = default);
        public record AppGroupFileResourceInfo : BaseDto<Guid>
        {
            public string FileUrl { get; set; } = null!;
            public long FileSize { get; set; }
            public string? FileExt { get; set; }
            public string Name { get; set; } = null!;
            public string Code { get; set; } = null!;
            public string? Description { get; set; }
            public Dictionary<string, object>? Properties { get; set; }
        }
    }
}

using System.ComponentModel.DataAnnotations;
using YS.Knife.Query;
using YS.Knife.Service;

namespace YS.Knife.AppRes
{
    public interface IAppTextResourceService
    {
        Task<string> GetContent(string key, CancellationToken cancellationToken);
        Task<StreamBody> Download(string key, CancellationToken cancellationToken);
        Task<PagedList<AppGroupTextResourceInfo>> Query(string group, LimitQueryInfo req, CancellationToken cancellationToken = default);

        public record AppGroupTextResourceInfo : BaseDto<Guid>
        {
            public string Content { get; set; } = null!;
            public string Name { get; set; } = null!;
            public string Code { get; set; } = null!;
            public string? Description { get; set; }
            public Dictionary<string, object>? Properties { get; set; }
        }

    }
}

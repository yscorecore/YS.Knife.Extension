using YS.Knife.Query;
using YS.Knife.Service;

namespace YS.Knife.CodeMapper
{
    public partial interface ICodeMapperManagerService<T>
    {
        Task<PagedList<CodeMapperDto>> QueryPagedList(LimitQueryInfo req, CancellationToken cancellationToken = default);

        Task SaveMappers(UpdateTargetMapperReq[] req, CancellationToken cancellationToken = default);
        public record CodeMapperDto : BaseDto<T>
        {
            public string Group { get; set; } = null!;

            public string SourceCode { get; set; } = null!;

            public string? SourceName { get; set; }

            public string? TargetCode { get; set; }

            public string? TargetName { get; set; }
        }
        public record UpdateTargetMapperReq : IdDto<T>
        {
            public string? TargetCode { get; set; }

            public string? TargetName { get; set; }
        }
    }
}

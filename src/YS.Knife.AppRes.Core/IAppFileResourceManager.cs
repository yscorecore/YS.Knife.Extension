using System.ComponentModel.DataAnnotations;
using YS.Knife.Service;
using static YS.Knife.AppRes.IAppFileResourceManager;

namespace YS.Knife.AppRes
{
    public interface IAppFileResourceManager :
        IQueryPageApi<AppFileResourceInfo>,
        ICreateApi<AddAppFileResourceDto, Guid>,
        IUpdateApi<EditAppFileResourceDto, Guid>,
        IDeleteApi<Guid>
    {
        public record AppFileResourceInfo : BaseDto<Guid>
        {
            public string FileUrl { get; set; } = null!;
            public long FileSize { get; set; }
            public string? FileExt { get; set; }
            public string Group { get; set; } = null!;
            public string Name { get; set; } = null!;
            public string Code { get; set; } = null!;
            public string? Description { get; set; }
            public Dictionary<string, object>? Properties { get; set; }
        }
        public record AddAppFileResourceDto
        {
            [StringLength(256)]
            public string FileUrl { get; set; } = null!;
            public long FileSize { get; set; }
            [StringLength(8)]
            public string? FileExt { get; set; }
            [StringLength(64)]
            [Required]
            public string Group { get; set; } = null!;

            [StringLength(64)]
            [Required]
            public string Name { get; set; } = null!;
            [StringLength(32)]
            [NameRule]
            public string Code { get; set; } = null!;

            [StringLength(256)]
            public string? Description { get; set; }
            public int Order { get; set; }

            public Dictionary<string, object>? Properties { get; set; }
        }
        public record EditAppFileResourceDto : IdDto<Guid>
        {
            [StringLength(256)]
            public string FileUrl { get; set; } = null!;
            public long FileSize { get; set; }
            [StringLength(8)]
            public string? FileExt { get; set; }

            [StringLength(64)]
            [Required]
            public string Name { get; set; } = null!;
            [StringLength(32)]
            [NameRule]
            public string Code { get; set; } = null!;

            [StringLength(256)]
            public string? Description { get; set; }
            public int Order { get; set; }

            public Dictionary<string, object>? Properties { get; set; }
        }
    }
}

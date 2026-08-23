using System.ComponentModel.DataAnnotations;
using YS.Knife.Service;
using static YS.Knife.AppRes.IAppTextResourceManager;

namespace YS.Knife.AppRes
{
    public interface IAppTextResourceManager :
      IQueryPageApi<AppTextResourceInfo>,
      ICreateApi<AddAppTextResourceDto, Guid>,
      IUpdateApi<EditAppTextResourceDto, Guid>,
      IDeleteApi<Guid>
    {
        public record AppTextResourceInfo : BaseDto<Guid>
        {
            public string Content { get; set; } = null!;
            public string Group { get; set; } = null!;
            public string Name { get; set; } = null!;
            public string Code { get; set; } = null!;
            public string? Description { get; set; }
            public Dictionary<string, object>? Properties { get; set; }
        }
        public record AddAppTextResourceDto
        {
            [Required]
            public string Content { get; set; } = null!;

            [StringLength(64)]
            [Required]
            public string Group { get; set; } = null!;

            [StringLength(64)]
            [Required]
            public string Name { get; set; } = null!;
            [StringLength(64)]
            public string Code { get; set; } = null!;

            [StringLength(256)]
            public string? Description { get; set; }
            public Dictionary<string, object>? Properties { get; set; }
        }
        public record EditAppTextResourceDto : IdDto<Guid>
        {
            [Required]
            public string Content { get; set; } = null!;
            [StringLength(64)]
            [Required]
            public string Group { get; set; } = null!;

            [StringLength(64)]
            [Required]
            public string Name { get; set; } = null!;
            [StringLength(64)]
            public string Code { get; set; } = null!;

            [StringLength(256)]
            public string? Description { get; set; }
            public Dictionary<string, object>? Properties { get; set; }
        }
    }
}

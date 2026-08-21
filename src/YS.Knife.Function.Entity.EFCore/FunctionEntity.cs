using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using YS.Knife.Entity;

namespace YS.Knife.Function.Entity.EFCore
{
    [Index(nameof(AppId))]
    [Index(nameof(AppId), nameof(Code), IsUnique = true)]
    [Index(nameof(AppId), nameof(ParentCode))]
    public class FunctionEntity : BaseEntity<Guid>
    {
        [StringLength(16)]
        [Required]
        public string Type { get; set; } = null!;
        [StringLength(64)]
        [Required]
        public string AppId { get; set; } = null!;
        [StringLength(64)]
        public string Name { get; set; } = null!;
        [StringLength(128)]
        [Required]
        public string Code { get; set; } = null!;
        [StringLength(256)]
        public string? Description { get; set; }
        [StringLength(128)]
        public string? ParentCode { get; set; }

        public int Sequence { get; set; }
        [JsonContent]
        public Dictionary<string, object>? Config { get; set; }

    }
}

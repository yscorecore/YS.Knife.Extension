using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using YS.Knife.Entity;

namespace YS.Knife.CodeMapper.Entity.EFCore
{
    [Index(nameof(Group))]
    [Index(nameof(Group), nameof(SourceCode), IsUnique = true)]
    public class CodeMapperEntity<T> : BaseEntity<T>
       where T : notnull
    {
        [Required]
        [StringLength(128)]
        public string Group { get; set; } = null!;
        [StringLength(64)]
        public string SourceCode { get; set; } = null!;
        [StringLength(64)]
        public string? SourceName { get; set; }
        [StringLength(64)]
        public string? TargetCode { get; set; }
        [StringLength(64)]
        public string? TargetName { get; set; }
    }
}

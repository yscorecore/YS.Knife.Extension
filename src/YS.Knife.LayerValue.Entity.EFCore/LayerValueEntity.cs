using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using YS.Knife.Entity;

namespace YS.Knife.LayerValue.Entity.EFCore
{
    [Index(nameof(Group), nameof(RoleCode), nameof(Key), IsUnique = true)]
    [Index(nameof(Group), nameof(RoleCode))]
    public class LayerValueEntity : BaseEntity<Guid>
    {
        [Required]
        [StringLength(64)]
        public string Group { get; set; } = null!;
        [Required]
        [StringLength(64)]
        public string RoleCode { get; set; } = null!;
        [Required]
        [StringLength(128)]
        public string Key { get; set; } = null!;
        [Required]
        public string Value { get; set; } = null!;
    }
}

using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using YS.Knife.Entity;

namespace YS.Knife.LayerSetting.Entity.EFCore
{
    [Index(nameof(Group), IsUnique = true)]
    public class SettingEntity : BaseEntity<Guid>
    {
        [StringLength(32)]
        public string Name { get; set; } = null!;

        [StringLength(64)]
        [Required]
        public string Group { get; set; } = null!;

        [StringLength(256)]
        public string? Description { get; set; }

        public string[] RoleProviders { get; set; } = null!;
        public List<SettingPropertyEntity> Properties { get; set; } = new();
    }
    public class SettingPropertyEntity : BaseEntity<Guid>
    {
        public Guid SettingId { get; set; }
        public SettingEntity setting { get; set; } = null!;

        [StringLength(64)]
        [Required]
        public string Key { get; set; } = null!;

        [StringLength(64)]
        public string Name { get; set; } = null!;

        [StringLength(256)]
        public string? Description { get; set; }

        public bool IsArray { get; set; }
        [StringLength(64)]
        [Required]
        public string Type { get; set; } = null!;

        [StringLength(1024)]
        public string? DataSource { get; set; }
    }
}

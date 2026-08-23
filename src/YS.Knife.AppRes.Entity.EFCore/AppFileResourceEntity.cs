using System.ComponentModel.DataAnnotations;

namespace YS.Knife.AppRes.Entity.EFCore
{
    public class AppFileResourceEntity : BaseResourceEntity
    {
        [StringLength(256)]
        public string FileUrl { get; set; } = null!;
        public long FileSize { get; set; }
        [StringLength(8)]
        public string? FileExt { get; set; }

    }
}

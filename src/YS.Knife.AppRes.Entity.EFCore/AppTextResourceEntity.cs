using Microsoft.EntityFrameworkCore;

namespace YS.Knife.AppRes.Entity.EFCore
{
    [Index(nameof(Group))]
    [Index(nameof(Group), nameof(Code))]
    [Index(nameof(Group), nameof(Name), IsUnique = true)]
    public class AppTextResourceEntity : BaseResourceEntity
    {
        public string Content { get; set; } = null!;
    }
}

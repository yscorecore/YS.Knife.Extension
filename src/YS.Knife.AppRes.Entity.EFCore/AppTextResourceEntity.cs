using Microsoft.EntityFrameworkCore;

namespace YS.Knife.AppRes.Entity.EFCore
{
    public class AppTextResourceEntity : AppResourceEntity
    {
        public string Content { get; set; } = null!;
    }
}

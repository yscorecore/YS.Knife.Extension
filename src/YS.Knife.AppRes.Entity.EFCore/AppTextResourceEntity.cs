using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace YS.Knife.AppRes.Entity.EFCore
{
    public class AppTextResourceEntity : AppResourceEntity
    {
        public string Content { get; set; } = null!;
        [StringLength(64)]
        public string? ContentType { get; set; } = "text/plain";
    }
}

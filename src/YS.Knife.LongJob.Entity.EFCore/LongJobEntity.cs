using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using YS.Knife.Entity;

namespace YS.Knife.LongJob.Entity.EFCore
{
    [Index(nameof(Type))]
    public class LongJobEntity : BaseEntity<Guid>
    {
        [Comment("任务类型")]
        [StringLength(64)]
        [Required]
        public string Type { get; set; } = null!;

        [Comment("任务描述")]
        [StringLength(256)]
        public string? Description { get; set; }

        [Comment("进度消息")]
        [StringLength(512)]
        public string? Message { get; set; }

        [Comment("进度百分比")]
        public double Percentage { get; set; }

        [Comment("是否已完成")]
        public bool Completed { get; set; }

        [Comment("是否成功")]
        public bool Success { get; set; }

        [Comment("附加资产")]
        [JsonContent]
        public object? Assets { get; set; }
    }
}

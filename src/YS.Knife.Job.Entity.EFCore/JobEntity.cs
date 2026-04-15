using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using YS.Knife.Entity;

namespace YS.Knife.Job.Entity.EFCore
{
    [Index(nameof(Key))]
    [Index(nameof(NextExecutionTime))]
    public class JobEntity<T> : BaseEntity<T>
        where T:notnull
    {
        [StringLength(64)]
        public string Key { get; set; } = null!;

        [Required]
        [StringLength(64)]
        public string Type { get; set; } = null!;
        public string? Argument { get; set; }

        public DateTime NextExecutionTime { get; set; }
        public DateTime? LastFailureTime { get; set; }
        public DateTime? LatestExecutionTime { get; set; }

        public int? MaxFailureCount { get; set; }

        public int? IncreaseSeconds { get; set; }

        public int TotalFailureCount { get; set; }
        [StringLength(8000)]
        public string? LastFailureReason { get; set; }
    }
}

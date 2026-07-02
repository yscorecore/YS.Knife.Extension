using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using YS.Knife.Entity;

namespace YS.Knife.ActiveKey.Entity.EFCore
{


    [Microsoft.EntityFrameworkCore.Index(nameof(Code))]
    public class ActiveKeyEntity<T> : BaseEntity<T>, ISoftDeleteEntity
      where T : notnull
    {
        [StringLength(256)]
        public string Code { get; set; } = null!;
        [StringLength(64)]
        public string? Owner { get; set; } = null!;
        [JsonContent]
        public Dictionary<string, object> Meta { get; set; } = new();
        public DateTime? LatestRequestTime { get; set; }
        [JsonContent]
        public Dictionary<string, object> RequestContext { get; set; } = new();
        public DateTime? LatestConsumeTime { get; set; }
        [JsonContent]
        public Dictionary<string, object> ConsumeContext { get; set; } = new();
        public bool IsDeleted { get; set; }

        //public ActiveKeyStatue Status { get; set; }
    }
}

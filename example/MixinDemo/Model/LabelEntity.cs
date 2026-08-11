using System.ComponentModel.DataAnnotations;
using YS.Knife.Entity;

namespace MixinDemo.Model
{
    public class LabelEntity : BaseEntity<int>
    {
        public string Name { get; set; } = null!;
        public string Desc { get; set; } = null!;
        public double Value { get; set; }
        public LabelStatus Status { get; set; } = LabelStatus.New;
    }

    public enum LabelStatus
    {
        [Display(Name = "新建")]
        New = 0,
        [Display(Name = "激活")]
        Active = 1,
        [Display(Name = "完成")]
        Complete = 2
    }
}

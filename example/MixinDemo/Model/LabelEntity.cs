using YS.Knife.Entity;

namespace MixinDemo.Model
{
    public class LabelEntity : BaseEntity<int>
    {
        public string Name { get; set; } = null!;
        public string Desc { get; set; } = null!;
    }
}

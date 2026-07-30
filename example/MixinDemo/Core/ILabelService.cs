using YS.Knife.Operations;
using YS.Knife.Service;
using static MixinDemo.Core.ILabelService;

namespace MixinDemo.Core
{
    [OperationArgument("name", "标签")]
    public interface ILabelService : IQueryPageApi<LabelInfo>,
        ICreateApi<CreateLabelInfo, int>,
        IDeleteApi<int>,
        IUpdateApi<UpdateLabelInfo, int>
    {

        public record LabelInfo : BaseDto<int>
        {
            public string Name { get; set; } = null!;
            public string Desc { get; set; } = null!;
        }
        public record CreateLabelInfo
        {
            public string Name { get; set; } = null!;
            public string Desc { get; set; } = null!;
        }
        public record UpdateLabelInfo : IdDto<int>
        {
            public string Name { get; set; } = null!;
            public string Desc { get; set; } = null!;
        }
    }
}

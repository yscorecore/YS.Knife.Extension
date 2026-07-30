using MixinDemo.Core;
using MixinDemo.Model;
using YS.Knife;
using YS.Knife.EFCore.Services;

namespace MixinDemo.Impl
{
    [Service(typeof(ILabelService))]
    [AutoConstructor]
    [Mixin(typeof(QueryApi<LabelEntity, ILabelService.LabelInfo>),
        typeof(CreateApi<LabelEntity, ILabelService.CreateLabelInfo, int>),
        typeof(UpdateApi<LabelEntity, ILabelService.UpdateLabelInfo, int>),
        typeof(DeleteApi<LabelEntity, int>)
        )]
#pragma warning disable FT50000 // The mapper is not being used
    [Mapper(typeof(LabelEntity), typeof(ILabelService.LabelInfo), MapperType = MapperType.Query)]
    [Mapper(typeof(ILabelService.CreateLabelInfo), typeof(LabelEntity), MapperType = MapperType.Convert)]
    [Mapper(typeof(ILabelService.UpdateLabelInfo), typeof(LabelEntity), MapperType = MapperType.Update)]
#pragma warning restore FT50000 // The mapper is not being used
    public partial class LabelService : ILabelService
    {
    }
}

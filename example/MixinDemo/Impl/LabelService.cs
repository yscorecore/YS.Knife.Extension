using System.Diagnostics.CodeAnalysis;
using MixinDemo.Core;
using MixinDemo.Model;
using YS.Knife;
using YS.Knife.EFCore.Services;
using static MixinDemo.Core.ILabelService;

namespace MixinDemo.Impl
{
    [Service(typeof(ILabelService))]
    [AutoConstructor]
    [Mixin(typeof(QueryApi<LabelEntity, LabelInfo>))]
    [Mixin(typeof(CreateApi<LabelEntity, CreateLabelInfo, int>))]
    [Mixin(typeof(UpdateApi<LabelEntity, UpdateLabelInfo, int>))]
    [Mixin(typeof(DeleteApi<LabelEntity, int>))]
    [Mapper(typeof(LabelEntity), typeof(LabelInfo), MapperType = MapperType.Query)]
    [Mapper(typeof(CreateLabelInfo), typeof(LabelEntity), MapperType = MapperType.Convert)]
    [Mapper(typeof(UpdateLabelInfo), typeof(LabelEntity), MapperType = MapperType.Update)]
    [SuppressMessage("FlyTiger.Mapper", "FT50000:The mapper is not being used")]
    public partial class LabelService : ILabelService
    {
    }
}

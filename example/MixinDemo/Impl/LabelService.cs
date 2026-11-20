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
    [Mixin(typeof(PrintService))]
    [SuppressMessage("FlyTiger.Mapper", "FT50000:The mapper is not being used")]
    public partial class LabelService : ILabelService
    {
        //public Task Delete(int[] ids, CancellationToken token = default)
        //{
        //    return Task.CompletedTask;
        //}
    }
    [Service(typeof(PrintService))]
    public class PrintService
    {
        public void Print()
        {
            Console.WriteLine("Print 1");
        }
    }
}

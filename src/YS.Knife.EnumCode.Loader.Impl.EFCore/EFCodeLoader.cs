
using Microsoft.EntityFrameworkCore;
using YS.Knife.Entity;
using YS.Knife.EnumCode.Loader.Entity.EFCore;

namespace YS.Knife.EnumCode.Loader.Impl.EFCore
{
    [Service]
    [AutoConstructor]
    [Mapper(typeof(EnumCodeEntity<Guid>), typeof(IEnumCodeService.CodeInfo), MapperType = MapperType.Convert)]
    public partial class EFCodeLoader : ICodeLoader
    {
        private readonly IEntityStore<EnumCodeEntity<Guid>> entityStore;

        public int Priority => 10000;

        public async Task<Dictionary<string, List<IEnumCodeService.CodeInfo>>> AllCodes()
        {
            var all = await entityStore.Current.FilterDeleted().ToListAsync();
            return all.GroupBy(p => p.Kind)
                 .ToDictionary(t => t.Key, t => t.OrderBy(t => t.Order).Select(tt => tt.To<IEnumCodeService.CodeInfo>()).ToList());
        }
    }

}

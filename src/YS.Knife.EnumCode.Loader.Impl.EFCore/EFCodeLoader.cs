
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using YS.Knife.Entity;
using YS.Knife.EnumCode.Loader.Entity.EFCore;
using static YS.Knife.EnumCode.IEnumCodeService;

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
    [Options]
    public class EnumCodeOptions
    {
        public string[] Assemblies { get; set; } = new string[0];

    }
    [Service]
    [Mapper(typeof(CodeInfo), typeof(TempCodeInfo), MapperType = MapperType.Convert)]
    [Mapper(typeof(TempCodeInfo), typeof(EnumCodeEntity<Guid>), MapperType = MapperType.All)]
    [AutoConstructor]
    public partial class EFCodeManager
    {
        private readonly IEntityStore<EnumCodeEntity<Guid>> entityStore;
        private readonly EnumCodeOptions options;

        public async Task SyncEnumCodeFromAssemblies(CancellationToken cancellationToken = default)
        {
            var dic = AssemblyEnumCodes.Instance.LoadAssemblyEnumCodes(options.Assemblies);
            var allCodes = dic.SelectMany(p => p.Value.Select(t => t.To<TempCodeInfo>(x => x.Kind = p.Key))).ToList();
            var full = await entityStore.Current.ToListAsync(cancellationToken);
            allCodes.To(full, CollectionUpdateMode.Append,
                p => new { p.Kind, p.Key }, p => new { p.Kind, p.Key },
                onAddItem: (o) => entityStore.Add((EnumCodeEntity<Guid>)o),
                onRemoveItem: null);
            await entityStore.SaveChangesAsync(cancellationToken);
        }
        internal record TempCodeInfo : CodeInfo
        {
            public string Kind { get; set; } = null!;
        }
    }
}

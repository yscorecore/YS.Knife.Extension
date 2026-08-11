using Microsoft.EntityFrameworkCore;
using YS.Knife.Entity;
using YS.Knife.Function.Core;
using YS.Knife.Function.Entity.EFCore;

namespace YS.Knife.Function.Impl.EFCore
{

    [Mapper(typeof(FunctionEntity), typeof(FunctionInfo), MapperType = MapperType.Query)]
    [Mapper(typeof(FunctionEntity), typeof(FunctionTreeInfo), MapperType = MapperType.Query)]
    [Mapper(typeof(FunctionInfo), typeof(FunctionEntity), MapperType = MapperType.BatchUpdate, CustomMappings = new string[]
    {
        "Config = $.Config"
    })]
    [Service]
    [AutoConstructor]
    public partial class FunctionManagerService : IFunctionManagerService
    {
        private readonly IEntityStore<FunctionEntity> functionEntityStore;

        public Task<List<FunctionInfo>> GetApps()
        {
            return functionEntityStore.Current
                    .Where(p => p.Code == p.AppId && string.IsNullOrEmpty(p.ParentCode))
                    .To<FunctionInfo>()
                    .ToListAsync();
        }

        public async Task<FunctionTreeInfo> GetFunctionTree(string appId)
        {
            var res = await functionEntityStore.Current
                   .Where(p => p.AppId == appId)
                   .OrderBy(p => p.Sequence)
                   .To<FunctionTreeInfo>()
                   .ToListAsync();
            return res.BuildTree(appId);
        }


        public async Task SaveFunctions(string appId, List<FunctionInfo> allFunctions)
        {
            var allFunctionInDb = await functionEntityStore.Current.Where(p => p.AppId == appId).ToListAsync();
            allFunctions.To(allFunctionInDb, CollectionUpdateMode.Update,
                (t) => functionEntityStore.Delete((FunctionEntity)t),
                (t) =>
                {
                    var val = (FunctionEntity)t;
                    val.AppId = appId;
                    functionEntityStore.Add(val);
                });
            await functionEntityStore.SaveChangesAsync();
        }
    }
}

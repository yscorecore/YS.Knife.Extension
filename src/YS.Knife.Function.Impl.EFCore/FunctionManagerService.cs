using System.Reflection;
using Microsoft.EntityFrameworkCore;
using YS.Knife.Entity;
using YS.Knife.Function.Entity.EFCore;
using YS.Knife.Function.Files;
using YS.Knife.Query;

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

        public async Task<FunctionTreeInfo> GetFunctionTree(string appId, CancellationToken cancellationToken = default)
        {
            var res = await functionEntityStore.Current
                   .Where(p => p.AppId == appId)
                   .OrderBy(p => p.Sequence)
                   .To<FunctionTreeInfo>()
                   .ToListAsync();
            return res.BuildTree(appId);
        }
        public Task<PagedList<FunctionInfo>> GetApps(LimitQueryInfo req, CancellationToken cancellationToken = default)
        {
            return functionEntityStore.Current
                   .Where(p => p.Code == p.AppId && string.IsNullOrEmpty(p.ParentCode))
                   .To<FunctionInfo>()
                   .QueryPageAsync(req, cancellationToken);
        }

        public async Task<List<FunctionInfo>> LoadFromFile(StreamBody file, CancellationToken cancellationToken)
        {
            var res = await AppInfo.LoadFromFile(file, cancellationToken);
            return res.ToFunctionModel();
        }

        public async Task DeleteApp(string appId, CancellationToken cancellationToken = default)
        {
            var res = await functionEntityStore.Current
                    .Where(p => p.AppId == appId)
                    .ToListAsync(cancellationToken);
            functionEntityStore.DeleteRange(res);
            await functionEntityStore.SaveChangesAsync(cancellationToken);
        }

        public async Task SaveFunctions(string appId, List<FunctionInfo> allFunctions, CancellationToken cancellationToken)
        {
            var allFunctionInDb = await functionEntityStore.Current.Where(p => p.AppId == appId).ToListAsync(cancellationToken);
            allFunctions.To(allFunctionInDb, CollectionUpdateMode.Update,
                (t) => functionEntityStore.Delete((FunctionEntity)t),
                (t) =>
                {
                    var val = (FunctionEntity)t;
                    val.AppId = appId;
                    functionEntityStore.Add(val);
                });
            await functionEntityStore.SaveChangesAsync(cancellationToken);
        }

        public async Task RefreshApiFunctions(CancellationToken cancellationToken = default)
        {
            var entry = Assembly.GetEntryAssembly();
            var appInfo = entry.FindAppInfo(AppDomain.CurrentDomain.GetAssemblies(), IsModule, IsAction);
            var appId = appInfo.AppId;
            var functions = appInfo.ToFunctionModel();
            await SaveFunctions(appId, functions, cancellationToken);
        }
        private static bool IsModule(Type type)
        {
            return type.GetCustomAttributesData().Any(t => t.AttributeType.FullName == "Microsoft.AspNetCore.Mvc.ApiControllerAttribute");
        }
        private static HashSet<string> AllHttpMethodAttributeTypes = new HashSet<string>()
        {
            "Microsoft.AspNetCore.Mvc.HttpGetAttribute",
            "Microsoft.AspNetCore.Mvc.HttpPostAttribute",
            "Microsoft.AspNetCore.Mvc.HttpPutAttribute",
            "Microsoft.AspNetCore.Mvc.HttpDeleteAttribute",
            "Microsoft.AspNetCore.Mvc.HttpHeadAttribute",
            "Microsoft.AspNetCore.Mvc.HttpPatchAttribute",
            "Microsoft.AspNetCore.Mvc.HttpOptionsAttribute",
        };
        private static bool IsAction(MethodInfo method)
        {
            return method.GetCustomAttributesData().Any(t => AllHttpMethodAttributeTypes.Contains(t.AttributeType.FullName));
        }
    }
}

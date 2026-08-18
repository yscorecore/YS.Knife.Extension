using System.Text.Json;
using YS.Knife.Function.Core;
using YS.Knife.LogicRoles;

namespace YS.Knife.Function.Impl.EFCore
{
    [AutoConstructor]
    [Service]
    public partial class FunctionPermissionService : IFunctionPermissionService
    {
        public const string PIPE = "-";
        private readonly ILayerService layerService;
        private readonly IFunctionManagerService functionManagerService;
        private readonly ILogicRoleService logicRoleService;
        private string[] GetAppRoleProviders(FunctionTreeInfo appInfo)
        {
            if (appInfo.Config != null)
            {
                if (appInfo.Config.TryGetValue("roleProviders", out var res) || appInfo.Config.TryGetValue("RoleProviders", out res))
                {
                    if (res is JsonElement je)
                    {
                        return je.EnumerateArray().Select(p => p.GetString()).ToArray();
                    }
                }
                return (res as string[]) ?? Array.Empty<string>();
            }
            return Array.Empty<string>();
        }
        internal static IEnumerable<string[]> SplitPipeArray(string[] all, string splitChar = PIPE)
        {
            List<string> segment = new List<string>();
            foreach (var item in all)
            {
                if (item == splitChar && segment.Count > 0)
                {
                    yield return segment.ToArray();
                    segment.Clear();
                }
                else
                {
                    segment.Add(item);
                }
            }
            if (segment.Count > 0)
            {
                yield return segment.ToArray();
            }
        }
        internal static string[] FilterPipe(string[] providerNamesWithPipe)
        {
            return providerNamesWithPipe.Where(p => !string.IsNullOrEmpty(p) && p != PIPE).ToArray();
        }

        public async Task<FunctionTreeInfo> GetPermissionTree(string appId, string functionCode)
        {
            var tree = await functionManagerService.GetFunctionTree(appId);
            var providers = GetAppRoleProviders(tree);
            var allRoleCodes = await logicRoleService.GetAllRoleCodes();

            //所有的分层数据
            var allLayerValues = await layerService.GetLayerValuesByRoleCodes(appId, allRoleCodes.FilterByProviders(providers));
            foreach (var group in SplitPipeArray(providers))
            {
                tree = GetAssignedTree(tree, group, allLayerValues);
            }
            if (string.IsNullOrEmpty(functionCode) || tree == null)
            {
                return tree;
            }
            else
            {
                return tree.ExpandTree().Where(p => p.Code == functionCode).FirstOrDefault();
            }
        }

        private FunctionTreeInfo GetAssignedTree(FunctionTreeInfo tree, string[] roleProviderNames, List<LayerValueInfo> allValues)
        {
            if (tree == null || roleProviderNames == null || roleProviderNames.Length == 0) return null;
            var assignMap = FilterRoleValuesByProviderNames(allValues, roleProviderNames)
                    .ToGroupValues()
                    .As<FunctionAssignObjectInfo>()
                    .ToDictionary(p => p.Key, p => p.Value);
            return tree.GetAssignedTree(assignMap);
        }
        IEnumerable<LayerValueInfo> FilterRoleValuesByProviderNames(List<LayerValueInfo> source, string[] providers)
        {
            foreach (var val in source)
            {
                foreach (var name in providers)
                {
                    if (val.RoleCode.StartsWith(name + "::"))
                    {
                        yield return val;
                    }
                }
            }
        }

        public async Task<bool> HasPermission(string appId, string funcitonCode)
        {
            var tree = await functionManagerService.GetFunctionTree(appId);
            //当前的在第一位，app在最后一位
            var functionCodeChains = GetFunctionCodeChains(appId, tree, funcitonCode);
            var providers = GetAppRoleProviders(tree);
            var allRoleCodes = await logicRoleService.GetAllRoleCodes();
            //所有的分层数据
            var allLayerValues = await layerService.GetLayerValues(appId, functionCodeChains, allRoleCodes.FilterByProviders(providers));

            bool hasPermission = true;
            foreach (var group in SplitPipeArray(providers))
            {
                hasPermission = HasPermission(appId, functionCodeChains, group, allLayerValues);
                if (hasPermission == false)
                {
                    return false;
                }
            }
            return hasPermission;

            string[] GetFunctionCodeChains(string appId, FunctionTreeInfo tree, string functionCode)
            {
                var res = new List<string>();
                var treeMap = tree.ExpandTree().ToDictionary(p => p.Code);
                var code = funcitonCode;
                while (code != null && treeMap.TryGetValue(code, out var func))
                {
                    res.Add(code);
                    if (!res.Contains(func.ParentCode))
                    {
                        code = func.ParentCode;
                    }
                    else
                    {
                        break;
                    }
                }
                return res.ToArray();
            }
        }
        private bool HasPermission(string appId, string[] functionCodeChains, string[] roleProviderNames, List<LayerValueInfo> allValues)
        {
            var res = FilterRoleValuesByProviderNames(allValues, roleProviderNames)
                    .ToGroupValues()
                    .As<FunctionAssignObjectInfo>()
                    .ToList();
            foreach (var code in functionCodeChains)
            {
                var assign = res.Where(p => p.Key == code).SingleOrDefault();
                if (assign != null)
                {
                    return assign.Value.Type != AssignType.Deny;
                }
            }
            return false;
        }

    }
}


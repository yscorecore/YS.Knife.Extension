using System.Text.Json;
using YS.Knife.LogicRoles;

namespace YS.Knife.Function.Impl.EFCore
{
    [AutoConstructor]
    [Service(typeof(IFunctionPermissionService))]
    [Service(typeof(IFunctionMockService))]
    public partial class FunctionPermissionService : IFunctionPermissionService, IFunctionMockService
    {
        public const string PIPE = "-";
        private readonly ILayerService layerService;
        private readonly IFunctionManagerService functionManagerService;
        private readonly ILogicRoleService logicRoleService;

        internal static RoleProviderInfo[] GetRoleProviders(FunctionInfo appInfo)
        {
            if (appInfo.Config != null)
            {
                if (appInfo.Config.TryGetValue("providers", out var res1) || appInfo.Config.TryGetValue("Providers", out res1))
                {
                    return NormalizeRoleProviders(res1.AsJsonObject<RoleProviderInfo[]>(ILayerService.JsonOptions));

                }
                if (appInfo.Config.TryGetValue("roleProviders", out var res) || appInfo.Config.TryGetValue("RoleProviders", out res))
                {
                    if (res is JsonElement je)
                    {
                        return NormalizeRoleProviders(SplitPipeArray(je.EnumerateArray().Select(p => p.GetString()).ToArray()).Select(p => new RoleProviderInfo
                        {
                            Providers = p,
                            Policy = LayerValuePolicy.Cover
                        }).ToArray());
                    }
                }
                return NormalizeRoleProviders(SplitPipeArray((res as string[]) ?? Array.Empty<string>()).Select(p => new RoleProviderInfo
                {
                    Providers = p,
                    Policy = LayerValuePolicy.Cover
                }).ToArray());
            }
            return Array.Empty<RoleProviderInfo>();

            static RoleProviderInfo[] NormalizeRoleProviders(IEnumerable<RoleProviderInfo> source)
            {
                return (source ?? Array.Empty<RoleProviderInfo>())
                    .Where(p => p != null)
                    .Select(p => p with
                    {
                        Providers = (p.Providers ?? Array.Empty<string>())
                            .Where(provider => !string.IsNullOrWhiteSpace(provider))
                            .ToArray()
                    })
                    .Where(p => p.Providers.Length > 0)
                    .ToArray();
            }
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

        public Task<FunctionTreeInfo> MockPermissionTree(string appId, string[] logicRoles, string stopProvider)
        {
            return GetPermissionTreeInternal(appId, null, logicRoles, stopProvider);
        }

        private async Task<FunctionTreeInfo> GetPermissionTreeInternal(string appId, string functionCode, string[] allRoleCodes, string stopProvider)
        {
            var tree = await functionManagerService.GetFunctionTree(appId);
            var roleProviders = GetRoleProviders(tree);

            var stopRoleProviderGroupIndex = roleProviders.Length - 1;
            var providers = roleProviders.SelectMany(p => p.Providers).ToList();
            if (!string.IsNullOrEmpty(stopProvider))
            {
                providers.Clear();
                for (var i = 0; i < roleProviders.Length; i++)
                {
                    var index = Array.IndexOf(roleProviders[i].Providers, stopProvider);
                    if (index >= 0)
                    {
                        stopRoleProviderGroupIndex = i;
                        var truncatedProviders = roleProviders[i].Providers.Take(index + 1).ToArray();
                        providers.AddRange(truncatedProviders);
                        roleProviders[i] = roleProviders[i] with { Providers = truncatedProviders };
                        break;
                    }
                    else
                    {
                        providers.AddRange(roleProviders[i].Providers);
                    }
                }
            }
            //所有的分层数据
            var allLayerValues = await layerService.GetLayerValuesByRoleCodes(appId, allRoleCodes.FilterByProviders(providers.ToArray()));

            for (var i = 0; i <= stopRoleProviderGroupIndex; i++)
            {
                tree = GetAssignedTree(tree, roleProviders[i].Providers, roleProviders[i].Policy, allLayerValues);
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

        public async Task<FunctionTreeInfo> GetPermissionTree(string appId, string functionCode)
        {
            var allRoleCodes = await logicRoleService.GetAllRoleCodes();
            return await GetPermissionTreeInternal(appId, functionCode, allRoleCodes, null);
        }

        private FunctionTreeInfo GetAssignedTree(FunctionTreeInfo tree, string[] roleProviderNames, LayerValuePolicy layerValuePolicy, List<LayerValueInfo> allValues)
        {
            if (tree == null || roleProviderNames == null || roleProviderNames.Length == 0) return null;
            if (layerValuePolicy == LayerValuePolicy.Cover)
            {
                var assignMap = FilterRoleValuesByProviderNames(allValues, roleProviderNames)
                  .ToGroupValues()
                  .As<FunctionAssignObjectInfo>()
                  .ToDictionary(p => p.Key, p => p.Value);
                return tree.GetAssignedTree(assignMap);
            }
            else
            {
                return FilterRoleValuesByProviderNames(allValues, roleProviderNames).GroupBy(p => new { p.RoleCode })
                     .Select(p => p.ToGroupValues().As<FunctionAssignObjectInfo>().ToDictionary(t => t.Key, t => t.Value))
                     .Select(t => tree.GetAssignedTree(t))
                     .Where(p => p != null)
                     .Select(p => p!)
                     .MergeTree(tree.Code);
            }

        }
        internal static IEnumerable<LayerValueInfo> FilterRoleValuesByProviderNames(List<LayerValueInfo> source, string[] providers)
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
            var roleProviders = GetRoleProviders(tree);
            var providers = roleProviders.SelectMany(p => p.Providers).ToArray();
            var allRoleCodes = await logicRoleService.GetAllRoleCodes();
            //所有的分层数据
            var allLayerValues = await layerService.GetLayerValues(appId, functionCodeChains, allRoleCodes.FilterByProviders(providers));

            bool hasPermission = true;
            foreach (var group in roleProviders)
            {
                hasPermission = HasPermission(appId, functionCodeChains, group.Providers, group.Policy, allLayerValues);
                if (hasPermission == false)
                {
                    return false;
                }
            }
            return hasPermission;

            string[] GetFunctionCodeChains(string appId, FunctionTreeInfo tree, string functionCode)
            {
                var res = new List<string>();
                if (tree == null) return res.ToArray();
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
        internal static bool HasPermission(string appId, string[] functionCodeChains, string[] roleProviderNames, LayerValuePolicy layerValuePolicy, List<LayerValueInfo> allValues)
        {
            //functionCodeChains 目前还小到大排序，sub3,sub2,sub1
            bool HasPermissionForRole(IEnumerable<LayerValueInfo> roleValues, string[] codeChains)
            {
                if (codeChains.Length == 0)
                {
                    return false;
                }
                var assignments = roleValues
                    .ToGroupValues()
                    .As<FunctionAssignObjectInfo>()
                    .ToList();
                var parentAllowInherrites = false;
                for (var i = codeChains.Length - 1; i >= 0; i--)
                {
                    var assign = assignments.SingleOrDefault(p => p.Key == codeChains[i]);
                    if (assign != null)
                    {
                        if (assign.Value.Type == AssignType.Allow)
                        {
                            parentAllowInherrites = false;
                        }
                        else if (assign.Value.Type == AssignType.AllowInherrites)
                        {
                            parentAllowInherrites = true;
                        }
                        else
                        {
                            //Deny
                            return false;
                        }

                    }
                    else
                    {
                        if (parentAllowInherrites)
                        {
                            continue;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                return true;
            }

            if (layerValuePolicy == LayerValuePolicy.Cover)
            {
                return HasPermissionForRole(
                    FilterRoleValuesByProviderNames(allValues, roleProviderNames), functionCodeChains);
            }
            else
            {
                return FilterRoleValuesByProviderNames(allValues, roleProviderNames)
                    .GroupBy(p => p.RoleCode)
                    .Any(p => HasPermissionForRole(p, functionCodeChains));

            }


        }


    }
}


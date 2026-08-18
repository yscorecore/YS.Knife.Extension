using YS.Knife.Function.Core;

namespace YS.Knife.Function
{
    public static class FunctionTreeExtensions
    {
        public static string[] GetAssignedFunctionCodes(this FunctionTreeInfo? tree, IDictionary<string, FunctionAssignObjectInfo> assignMap)
        {
            if (tree == null) return Array.Empty<string>();
            var assignedTree = tree.GetAssignedTree(assignMap);
            if (assignedTree == null) return Array.Empty<string>();
            return assignedTree.ExpandTree().Select(p => p.Code).ToArray();
        }
        public static FunctionTreeInfo? GetAssignedTree(this FunctionTreeInfo? tree, IDictionary<string, FunctionAssignObjectInfo> assignMap)
        {
            if (tree == null) return null;
            return CreateFunctionInfo(false, tree);
            FunctionTreeInfo? CreateFunctionInfo(bool parentAllow, FunctionTreeInfo tree)
            {
                AssignType assignType = assignMap.TryGetValue(tree.Code, out var assignInfo) ?
                    assignInfo.Type : (parentAllow ? AssignType.AllowInherrites : AssignType.Deny);

                if (assignType != AssignType.Deny)
                {
                    var functionInfo = tree with { };
                    if (tree.SubItems != null)
                    {
                        var subItems = tree.SubItems.Select(p => CreateFunctionInfo(assignType == AssignType.AllowInherrites, p)).Where(p => p != null).ToList();
                        functionInfo.SubItems = subItems;
                    }
                    return functionInfo;
                }
                else
                {
                    return null;
                }
            }

        }
        public static FunctionTreeInfo? BuildTree(this IEnumerable<FunctionTreeInfo> functionTreeInfos, string appId)
        {
            var allFunctionTree = functionTreeInfos.ToDictionary(p => p.Code, p => p);
            foreach (var (_, v) in allFunctionTree)
            {
                if (!string.IsNullOrEmpty(v.ParentCode) && allFunctionTree.TryGetValue(v.ParentCode, out var parent))
                {
                    if (parent.SubItems == null)
                    {
                        parent.SubItems = new List<FunctionTreeInfo> { v };
                    }
                    else
                    {
                        parent.SubItems.Add(v);
                    }
                }
            }
            return allFunctionTree.TryGetValue(appId, out var res) ? res : null;
        }
        public static FunctionTreeInfo? BuildTree(this IDictionary<string, FunctionTreeInfo> allFunctionTree, string appId)
        {
            foreach (var (_, v) in allFunctionTree)
            {
                if (allFunctionTree.TryGetValue(v.ParentCode, out var parent))
                {
                    if (parent.SubItems == null)
                    {
                        parent.SubItems = new List<FunctionTreeInfo> { v };
                    }
                    else
                    {
                        parent.SubItems.Add(v);
                    }
                }
            }
            return allFunctionTree.TryGetValue(appId, out var res) ? res : null;
        }
        public static List<FunctionTreeInfo> ExpandTree(this FunctionTreeInfo functionTree)
        {
            var res = new List<FunctionTreeInfo>();
            Expand(functionTree);
            return res;
            void Expand(FunctionTreeInfo current)
            {
                res.Add(current);
                if (current.SubItems != null)
                {
                    foreach (var item in current.SubItems)
                    {
                        Expand(item);
                    }
                }
            }
        }
    }
}

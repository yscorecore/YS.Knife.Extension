using YS.Knife.Function.Core;

namespace YS.Knife.Function
{
    public static class FunctionTreeExtensions
    {
        public static FunctionTreeInfo BuildTree(this IEnumerable<FunctionTreeInfo> functionTreeInfos, string appId)
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
        public static FunctionTreeInfo BuildTree(this IDictionary<string, FunctionTreeInfo> allFunctionTree, string appId)
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

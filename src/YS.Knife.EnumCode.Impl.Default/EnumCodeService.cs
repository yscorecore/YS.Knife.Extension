using static YS.Knife.EnumCode.IEnumCodeService;
namespace YS.Knife.EnumCode.Impl.Default
{

    [AutoConstructor]
    [Service]
    public partial class EnumCodeService : IEnumCodeService
    {
        private readonly IEnumerable<ICodeLoader> loaders;
        public async Task<Dictionary<string, List<CodeInfo>>> GetAllCodes()
        {
            Dictionary<string, List<CodeInfo>> all = null;
            foreach (var loader in loaders.OrderBy(p => p.Priority))
            {
                var codes = await loader.AllCodes();
                if (all == null)
                {
                    all = codes;
                }
                else
                {
                    foreach (var code in codes)
                    {
                        if (all.TryGetValue(code.Key, out var current))
                        {
                            all[code.Key] = MergeCodes(current, code.Value);
                        }
                        else
                        {
                            all[code.Key] = code.Value;
                        }
                    }
                }
            }
            return all ?? new Dictionary<string, List<IEnumCodeService.CodeInfo>>();
        }
        private List<CodeInfo> MergeCodes(List<CodeInfo> first, List<CodeInfo> second)
        {
            var dic = first.ToDictionary(p => p.Key);
            foreach (var item in second)
            {
                dic[item.Key] = item;
            }
            return dic.Values.OrderBy(p => p.Order).ToList();
        }
    }
}

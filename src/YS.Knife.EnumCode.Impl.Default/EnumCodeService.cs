using System;
using static YS.Knife.EnumCode.IEnumCodeService;
namespace YS.Knife.EnumCode.Impl.Default
{

    [AutoConstructor]
    [Service]
    public partial class EnumCodeService : IEnumCodeService
    {
        private readonly IEnumerable<ICodeLoader> loaders;
        private readonly EnumCodeOptions options;
        public async Task<Dictionary<string, List<CodeInfo>>> GetAllCodes()
        {
            Dictionary<string, List<CodeInfo>> all = new Dictionary<string, List<CodeInfo>>();
            foreach (var loader in loaders.OrderBy(p => p.Priority))
            {
                var codes = await loader.AllCodes();
                foreach (var code in codes)
                {
                    var key = code.Key.WithStyle(options.NameStyle);
                    if (all.TryGetValue(key, out var current))
                    {
                        all[key] = MergeCodes(current, code.Value);
                    }
                    else
                    {
                        all[key] = code.Value;
                    }
                }
            }
            return all;
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

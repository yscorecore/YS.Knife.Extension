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
            foreach (var loader in loaders)
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
                        all[code.Key] = code.Value;
                    }
                }
            }
            return all ?? new Dictionary<string, List<IEnumCodeService.CodeInfo>>();
        }
    }
}

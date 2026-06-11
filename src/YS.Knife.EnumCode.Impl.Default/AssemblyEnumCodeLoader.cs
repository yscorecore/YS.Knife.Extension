using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static YS.Knife.EnumCode.IEnumCodeService;

namespace YS.Knife.EnumCode.Impl.Default
{
    [AutoConstructor]
    [Service(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
    public partial class AssemblyEnumCodeLoader : ICodeLoader
    {
        private readonly EnumCodeOptions options;
        [AutoConstructorIgnore]
        private Lazy<Dictionary<string, List<CodeInfo>>> cache;

        public int Priority => 0;

        [AutoConstructorInitialize]
        void Init()
        {
            cache = new Lazy<Dictionary<string, List<CodeInfo>>>(() => AssemblyEnumCodes.Instance.LoadAssemblyEnumCodes(options.Assemblies), true);
        }

      
        public Task<Dictionary<string, List<CodeInfo>>> AllCodes()
        {
            return Task.FromResult(cache.Value);
        }
    }
}

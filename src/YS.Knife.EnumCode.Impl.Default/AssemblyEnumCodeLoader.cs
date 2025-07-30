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
        private readonly AssemblyEnumCodeOptions options;
        [AutoConstructorIgnore]
        private Lazy<Dictionary<string, List<CodeInfo>>> cache;

        [AutoConstructorInitialize]
        void Init()
        {
            cache = new Lazy<Dictionary<string, List<CodeInfo>>>(() => LoadDataInternal(), true);
        }

        Dictionary<string, List<CodeInfo>> LoadDataInternal()
        {
            return options.Assemblies.Select(p => Assembly.Load(p))
                 .SelectMany(p => p.GetTypes().Where(x => x.IsEnum && x.IsPublic))
                  .ToDictionary(p => p.Name.ToLowerInvariant(), p => LoadEnumTypeCodes(p));
        }
        List<CodeInfo> LoadEnumTypeCodes(Type enumType)
        {
            return enumType.GetFields(BindingFlags.Static | BindingFlags.Public).Select(p => CreateCodeInfoFromEnumField(p))
                .OrderBy(p => p.Order).ToList();
        }
        CodeInfo CreateCodeInfoFromEnumField(FieldInfo fieldInfo)
        {
            var dis = fieldInfo.GetCustomAttribute<DisplayAttribute>();
            var des = fieldInfo.GetCustomAttribute<DescriptionAttribute>();
            return new CodeInfo
            {
                Key = Convert.ToInt32(fieldInfo.GetValue(null)),
                Display = dis?.Name ?? des?.Description ?? fieldInfo.Name,
                Description = dis?.Description,
                Group = dis?.GroupName,
                Order = dis?.Order ?? 0
            };
        }

        public Task<Dictionary<string, List<CodeInfo>>> AllCodes()
        {
            return Task.FromResult(cache.Value);
        }
    }
}

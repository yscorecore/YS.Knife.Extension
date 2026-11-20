using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using static YS.Knife.EnumCode.IEnumCodeService;

namespace YS.Knife.EnumCode
{
    [SingletonPattern]
    public partial class AssemblyEnumCodes
    {
        public Dictionary<string, List<CodeInfo>> LoadAssemblyEnumCodes(string[] assemblies)
        {
            return assemblies.Select(p => Assembly.Load(p))
                 .SelectMany(p => p.GetTypes().Where(x => x.IsEnum && x.IsPublic))
                  .ToDictionary(p => p.Name, p => LoadEnumTypeCodes(p), StringComparer.InvariantCultureIgnoreCase);
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
                Name = fieldInfo.Name,
                Display = dis?.Name ?? des?.Description ?? fieldInfo.Name,
                Description = dis?.Description,
                Group = dis?.GroupName,
                Order = dis?.Order ?? 0
            };
        }
    }
}

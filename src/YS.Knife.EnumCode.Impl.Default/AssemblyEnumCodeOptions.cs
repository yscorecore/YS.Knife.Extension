using System.ComponentModel.DataAnnotations;

namespace YS.Knife.EnumCode.Impl.Default
{
    [Options]
    public class AssemblyEnumCodeOptions
    {
        [Required(ErrorMessage = "没有配置包含枚举程序集")]
        [MinLength(1, ErrorMessage = "至少需要配置一个枚举程序集")]
        public string[] Assemblies { get; set; }


    }
}

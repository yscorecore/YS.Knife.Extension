using System.ComponentModel.DataAnnotations;

namespace YS.Knife.FileManager.Impl.EFCore
{
    [Options]
    public class CloudFileOptions
    {

        public string MainLogicRoleProvider { get; set; }

        [Required]
        public string[] LogicRoleProviders { get; set; } = new string[0];
    }
}

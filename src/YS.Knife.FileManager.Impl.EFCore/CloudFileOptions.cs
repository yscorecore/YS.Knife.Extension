using System.ComponentModel.DataAnnotations;

namespace YS.Knife.FileManager.Impl.EFCore
{
    [Options]
    public class CloudFileOptions
    {
        [Required]
        public string[] LogicRoleProviders { get; set; } = new string[0];
    }
}

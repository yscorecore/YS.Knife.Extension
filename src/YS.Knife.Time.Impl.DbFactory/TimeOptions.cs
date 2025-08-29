
using System.ComponentModel.DataAnnotations;

namespace YS.Knife.Time.Impl.EFDatabaseFacade
{
    [Options]
    public record TimeOptions
    {
        [Required(AllowEmptyStrings = false)]
        public string NowSqlScript { get; set; } = "select current_timestamp";
    }
}

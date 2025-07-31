using System.ComponentModel.DataAnnotations;

namespace YS.Knife.Import.Impl.Base
{
    [Options]
    public class ImportOptions
    {
        public string DataFolder { get; set; } = "import_data";
        //过期时长，最大长度, 最长的导入时间
        [Range(1, 60 * 60 * 24)]
        public int ExpiredIn { get; set; } = 7200;
    }
}

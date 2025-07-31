
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using YS.Knife.Metadata;

namespace MetadataDemo
{
    public class Program : YS.Knife.Hosting.KnifeWebHost
    {
        public Program(string[] args) : base(args)
        {
        }
        public static void Main(string[] args)
        {
            new Program(args).Run();
        }
    }
    [Metadata("DailyReports")]
    [DisplayName("日度报表")]
    public record ReportInfo
    {
        [DisplayName("报表Id")]
        [ScaffoldColumn(false)]
        public Guid Id { get; set; }
        [DisplayName("分类")]
        public string Category { get; set; }
        [DisplayName("编码")]
        [DisplayFormat(DataFormatString = "FileUrl")]
        [EditorSource("abc")]
        public string Code { get; set; }
        [DisplayName("名称")]
        public string Name { get; set; }
        [DisplayName("描述")]
        public string Description { get; set; }
        [DisplayName("参数信息")]
        public Dictionary<string, object> Arguments { get; set; }
    }
}

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using YS.Knife.Hosting;
using YS.Knife.Metadata;

namespace MetadataDemo
{
    [YS.Knife.ExposeApi(typeof(IMetadataService))]
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddAllKnifeServices(builder.Configuration);
            builder.Services.AddControllers();


            builder.Services.AddSwaggerGen();
            var app = builder.Build();
            app.MapControllers();
            app.UseSwagger();
            app.UseSwaggerUI();

            app.Run();
        }
    }

    [Metadata("DailyReports")]
    [DisplayName("日度报表")]
    public record ReportInfo
    {
        [Display(Name = "报表Id", Description = "报表Id描述", Order = 100)]
        [DisplayName("报表Id")]
        [ScaffoldColumn(false)]
        public Guid Id { get; set; }

        [DisplayName("分类")]
        public string Category { get; set; }

        [DisplayName("编码")]
        [DisplayFormat(DataFormatString = "FileUrl")]
        public string Code { get; set; }

        [DisplayName("名称")]
        public string Name { get; set; }

        [DisplayName("描述")]
        public string Description { get; set; }

        [DisplayName("参数信息")]
        public Dictionary<string, object> Arguments { get; set; }
    }
}

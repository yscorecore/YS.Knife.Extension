using System.ComponentModel.DataAnnotations;
using YS.Knife.Metadata;

namespace MetadataDemo
{
    [Metadata]
    public class WeatherForecast
    {
        [Display(Name = "日期", Order = 0, Description = "日期描述")]
        public DateOnly Date { get; set; }
        [Display(Name = "温度C", Order = 1)]
        public int TemperatureC { get; set; }
        [Display(Name = "温度F", Order = 2)]
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
        [Display(Name = "摘要", Order = 3)]
        public string? Summary { get; set; }
    }
}

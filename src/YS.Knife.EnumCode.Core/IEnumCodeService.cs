using System.Text.Json.Serialization;
using YS.Knife.Operations;

namespace YS.Knife.EnumCode
{
    public interface IEnumCodeService
    {
        [Operation("all", "获取所有的枚举值")]
        Task<Dictionary<string, List<CodeInfo>>> GetAllEnumCode();

        public record CodeInfo
        {
            public int Key { get; set; }

            public string Name { get; set; }
            public string Display { get; set; }
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
            public string Description { get; set; }
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
            public string Group { get; set; }
            [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
            public int Order { get; set; }

        }
    }
}

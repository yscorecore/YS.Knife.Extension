using System.Text.Json.Serialization;

namespace YS.Knife.EnumCode
{
    public interface IEnumCodeService
    {
        Task<Dictionary<string, List<CodeInfo>>> GetAllCodes();

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

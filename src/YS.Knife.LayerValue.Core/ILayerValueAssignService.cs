using System.ComponentModel.DataAnnotations;

namespace YS.Knife.Function
{
    public interface ILayerValueAssignService
    {
        public Task AssignByRole(LayerValueAssignByRoleInfo dto);

        Task<Dictionary<string, object>> GetLayerValueByRole(string group, string roleCode);

        public Task AssignByKey(LayerValueAssginByKeyInfo dto);

        Task<Dictionary<string, object>> GetLayerValueByKey(string group, string key);
        public record LayerValueAssignByRoleInfo
        {
            public string Group { get; set; } = null!;
            public string RoleCode { get; set; } = null!;
            public Dictionary<string, object> KeyValues { get; set; } = new();
        }
        public record LayerValueAssginByKeyInfo
        {
            public string Group { get; set; } = null!;
            public string Key { get; set; } = null!;
            public Dictionary<string, object> RoleValues { get; set; } = new();

        }
    }
}

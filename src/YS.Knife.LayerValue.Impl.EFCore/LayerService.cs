using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using YS.Knife.Entity;
using YS.Knife.LayerValue.Entity.EFCore;

namespace YS.Knife.Function.Impl.EFCore
{
    [Service]
    [AutoConstructor]
    public partial class LayerService : ILayerService
    {
        private readonly IEntityStore<LayerValueEntity> layerContext;
        public async Task<List<LayerValueInfo>> GetLayerValuesByKeys(string group, string[] keys)
        {
            var roleCodeLayerValue = await layerContext.Current
                .Where(s => s.Group == group && keys.Contains(s.Key)).OrderBy(p => p.Key).ToListAsync();
            return roleCodeLayerValue.Select(p => new LayerValueInfo { Key = p.Key, RoleCode = p.RoleCode, Value = p.Value }).ToList();
        }
        public async Task<List<LayerValueInfo>> GetLayerValues(string group, string[] keys, string[] roleCodes)
        {
            var roleCodeLayerValue = await layerContext.Current
                .Where(s => s.Group == group && keys.Contains(s.Key) && roleCodes.Contains(s.RoleCode)).ToListAsync();
            var res = roleCodeLayerValue.Select(p => new LayerValueInfo { Key = p.Key, RoleCode = p.RoleCode, Value = p.Value }).ToList();
            var roleMap = roleCodes.Select((item, index) => (item, index)).ToDictionary(s => s.item, s => s.index);
            return res.OrderBy(p => p.Key).ThenByDescending(s => roleMap[s.RoleCode]).ToList();
        }



        public async Task<List<LayerValueInfo>> GetLayerValuesByRoleCodes(string group, string[] roleCodes)
        {
            var roleCodeLayerValue = await layerContext.Current
               .Where(s => s.Group == group && roleCodes.Contains(s.RoleCode)).ToListAsync();
            var res = roleCodeLayerValue.Select(p => new LayerValueInfo { Key = p.Key, RoleCode = p.RoleCode, Value = p.Value }).ToList();
            var roleMap = roleCodes.Select((item, index) => (item, index)).ToDictionary(s => s.item, s => s.index);
            return res.OrderBy(p => p.Key).ThenByDescending(s => roleMap[s.RoleCode]).ToList();

        }
    }
}

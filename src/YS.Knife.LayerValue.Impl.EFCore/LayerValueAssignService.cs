using Microsoft.EntityFrameworkCore;
using YS.Knife.Entity;
using YS.Knife.Function;
using YS.Knife.LayerValue.Entity.EFCore;

namespace YS.Knife.LayerValue.Impl.EFCore
{
    [Service]
    [AutoConstructor]
    [Mapper(typeof(LayerValueInfo), typeof(LayerValueEntity), MapperType = MapperType.BatchUpdate)]
    public partial class LayerValueAssignService : ILayerValueAssignService
    {
        private readonly IEntityStore<LayerValueEntity> layerContext;
        public async Task AssignByKey(ILayerValueAssignService.LayerValueAssginByKeyInfo dto)
        {
            var current = await layerContext.Current.Where(p => p.Group == dto.Group && p.Key == dto.Key).ToListAsync();
            dto.RoleValues.Select(p => new LayerValueInfo { Key = dto.Key, RoleCode = p.Key, Value = p.Value.ToJsonText(ILayerService.JsonOptions) })
                .To(current, CollectionUpdateMode.Update,
                (t) => layerContext.Delete((LayerValueEntity)t),
                (t) => { ((LayerValueEntity)t).Group = dto.Group; });
            await layerContext.SaveChangesAsync();
        }
        public async Task AssignByRole(ILayerValueAssignService.LayerValueAssignByRoleInfo dto)
        {
            var current = layerContext.Current.Where(p => p.Group == dto.Group && p.RoleCode == dto.RoleCode).ToList();
            dto.KeyValues.Select(p => new LayerValueInfo { Key = p.Key, RoleCode = dto.RoleCode, Value = p.Value.ToJsonText(ILayerService.JsonOptions) })
                .To(current, CollectionUpdateMode.Update,
                (t) => layerContext.Delete((LayerValueEntity)t),
                (t) => { ((LayerValueEntity)t).Group = dto.Group; });
            await layerContext.SaveChangesAsync();
        }
        public async Task<List<LayerValueInfo<object>>> GetLayerValueByKey(string group, string key)
        {
            var res = await layerContext.Current.Where(p => p.Group == group && p.Key == key)
                 .Select(p => new LayerValueInfo { Key = p.Key, RoleCode = p.RoleCode, Value = p.Value })
                 .ToListAsync();
            return res.Select(p => p.ToLayerValueInfo<object>()).ToList();
        }
        public async Task<List<LayerValueInfo<object>>> GetLayerValueByRole(string group, string roleCode)
        {
            var res = await layerContext.Current.Where(p => p.Group == group && p.RoleCode == roleCode)
                 .Select(p => new LayerValueInfo { Key = p.Key, RoleCode = p.RoleCode, Value = p.Value })
                 .ToListAsync();
            return res.Select(p => p.ToLayerValueInfo<object>()).ToList();
        }
    }
}

using Microsoft.EntityFrameworkCore;
using YS.Knife.Entity;
using YS.Knife.Function;
using YS.Knife.LayerValue.Entity.EFCore;

namespace YS.Knife.LayerValue.Impl.EFCore
{
    [Service]
    [AutoConstructor]
    public partial class LayerValueAssignService : ILayerValueAssignService
    {
        private readonly IEntityStore<LayerValueEntity> layerContext;
        public async Task AssignByKey(ILayerValueAssignService.LayerValueAssginByKeyInfo dto)
        {
            var current = layerContext.Current.Where(p => p.Group == dto.Group && p.Key == dto.Key).ToList();
            var currentDic = current.ToDictionary(p => p.RoleCode);
            var needAdd = dto.RoleValues.Keys.Except(currentDic.Keys).ToList();
            var needDelete = currentDic.Keys.Except(dto.RoleValues.Keys).ToList();
            var needUpdate = currentDic.Keys.Intersect(dto.RoleValues.Keys).ToList();
            layerContext.DeleteRange(currentDic.Where(p => needDelete.Contains(p.Key)).Select(p => p.Value));
            layerContext.AddRange(dto.RoleValues.Where(p => needAdd.Contains(p.Key)).Select(p => new LayerValueEntity { Group = dto.Group, Key = dto.Key, RoleCode = p.Key, Value = p.Value.ToJsonText(ILayerService.JsonOptions) }));
            needUpdate.ForEach(p => currentDic[p].Value = dto.RoleValues[p].ToJsonText(ILayerService.JsonOptions));
            await layerContext.SaveChangesAsync();
        }
        public async Task AssignByRole(ILayerValueAssignService.LayerValueAssignByRoleInfo dto)
        {
            var current = layerContext.Current.Where(p => p.Group == dto.Group && p.RoleCode == dto.RoleCode).ToList();
            var currentDic = current.ToDictionary(p => p.Key);
            var needAdd = dto.KeyValues.Keys.Except(currentDic.Keys).ToList();
            var needDelete = currentDic.Keys.Except(dto.KeyValues.Keys).ToList();
            var needUpdate = currentDic.Keys.Intersect(dto.KeyValues.Keys).ToList();
            layerContext.DeleteRange(currentDic.Where(p => needDelete.Contains(p.Key)).Select(p => p.Value));
            layerContext.AddRange(dto.KeyValues.Where(p => needAdd.Contains(p.Key)).Select(p => new LayerValueEntity { Group = dto.Group, Key = p.Key, RoleCode = dto.RoleCode, Value = p.Value.ToJsonText(ILayerService.JsonOptions) }));
            needUpdate.ForEach(p => currentDic[p].Value = dto.KeyValues[p].ToJsonText(ILayerService.JsonOptions));
            await layerContext.SaveChangesAsync();
        }
        public async Task<Dictionary<string, object>> GetLayerValueByKey(string group, string key)
        {
            var res = await layerContext.Current.Where(p => p.Group == group && p.Key == key)
                 .Select(p => new LayerValueInfo { Key = p.Key, RoleCode = p.RoleCode, Value = p.Value })
                 .ToListAsync();
            return res.Select(p => p.ToLayerValueInfo<object>()).ToDictionary(p => p.RoleCode, p => p.Value);
        }
        public async Task<Dictionary<string, object>> GetLayerValueByRole(string group, string roleCode)
        {
            var res = await layerContext.Current.Where(p => p.Group == group && p.RoleCode == roleCode)
                 .Select(p => new LayerValueInfo { Key = p.Key, RoleCode = p.RoleCode, Value = p.Value })
                 .ToListAsync();
            return res.Select(p => p.ToLayerValueInfo<object>()).ToList().ToDictionary(p => p.Key, p => p.Value);
        }
    }
}

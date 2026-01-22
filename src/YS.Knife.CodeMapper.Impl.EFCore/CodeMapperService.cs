using System.Linq;
using Microsoft.EntityFrameworkCore;
using YS.Knife.CodeMapper.Entity.EFCore;
using YS.Knife.Entity;

namespace YS.Knife.CodeMapper.Impl.EFCore
{
    [Service]
    [AutoConstructor]
    public partial class CodeMapperService : ICodeMapperService
    {
        private readonly IEntityStore<CodeMapperEntity<Guid>> codeMapperEntity;
        public async Task<List<MapperResult<TData>>> MapAll<TData, TSourceKey>(IEnumerable<TData> source, string group, Func<TData, TSourceKey> sourceKeyFun, Func<TData, string>? sourceNameFun, bool autoSyncSourceData = true, CancellationToken token = default)
        {
            var tempData = source.Select(p => new
            {
                Data = p,
                SourceCode = Convert.ToString(sourceKeyFun(p)) ?? string.Empty,
                SourceName = sourceNameFun?.Invoke(p)
            }).ToList();
            var allSourceCodes = tempData.Select(p => p.SourceCode).ToHashSet();
            var allDataInDb = await codeMapperEntity.Current
                .Where(p => p.Group == group && allSourceCodes.Contains(p.SourceCode))
                .Select(p => new
                {
                    p.SourceCode,
                    p.SourceName,
                    p.TargetCode,
                    p.TargetName
                }).ToListAsync();
            var allDataDic = allDataInDb.ToDictionary(p => p.SourceCode);
            if (autoSyncSourceData)
            {
                var newMapperData = tempData.Where(p => !allDataDic.ContainsKey(p.SourceCode))
                    .Select(p => new CodeMapperEntity<Guid>
                    {
                        SourceCode = p.SourceCode,
                        SourceName = p.SourceName,
                    }).ToList();
                if (newMapperData.Count > 0)
                {
                    codeMapperEntity.AddRange(newMapperData);
                    await codeMapperEntity.SaveChangesAsync(token);
                }
            }
            return tempData.Select(p =>
            {
                if (allDataDic.TryGetValue(p.SourceCode, out var d))
                {
                    if (d.TargetCode == null)
                    {
                        return new MapperResult<TData>(p.Data, true, d.TargetCode, d.TargetName);
                    }
                    else
                    {
                        return new MapperResult<TData>(p.Data, false, default, default);
                    }
                }
                return new MapperResult<TData>(p.Data, false, default, default);

            }).ToList();


        }

    }
}

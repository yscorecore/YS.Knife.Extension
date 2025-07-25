
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;

namespace YS.Knife.Metadata.Impl.Mvc
{
    [Service]
    [FlyTiger.AutoConstructor]
    public partial class MetadataService : IMetadataService
    {
        private readonly IModelMetadataProvider modelMetadataProvider;
        private readonly IOptions<MetadataOptions> metadataOptions;

        public Task<MetadataInfo> GetMetadataInfo(string name, CancellationToken cancellationToken = default)
        {
            if (metadataOptions.Value.Metas.TryGetValue(name, out var type))
            {
                return Task.FromResult(modelMetadataProvider.GetMetadataInfoFromType(type));
            }
            throw new Exception($"Can not find data meta by name '{name}'.");

        }

        public Task<List<string>> ListAllNames(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(metadataOptions.Value.Metas.Keys.ToList());
        }
    }
}

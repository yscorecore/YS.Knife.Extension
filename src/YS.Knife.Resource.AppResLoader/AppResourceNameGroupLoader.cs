using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using YS.Knife.AppRes.Entity.EFCore;
using YS.Knife.Entity;

namespace YS.Knife.Resource.AppFileResLoader
{
    [Service]
    [AutoConstructor]
    public partial class AppResourceNameGroupLoader : IResourceLoader
    {
        private static readonly Regex regex = new Regex("^(<?c>.+)@(?<g>.+)$");
        public int Priority => 12000;
        private readonly IEntityStore<AppResourceEntity> entityStore;
        private readonly AppResourceOptions options;
        private readonly HttpClient httpClient;
        public bool CanLoad(string templateUri)
        {
            return regex.IsMatch(templateUri);
        }
        public async Task<Stream> LoadTemplate(string templateUri)
        {
            var match = regex.Match(templateUri);
            var group = match.Groups["g"].Value;
            var code = match.Groups["c"].Value;
            var appresourceEntity = await entityStore.Current.Where(p => p.Group == group && p.Code == code).FindOrThrowAsync();
            return await AppResourceLoader.LoadAppSourceEntity(appresourceEntity, httpClient, options, default);
        }
    }
}

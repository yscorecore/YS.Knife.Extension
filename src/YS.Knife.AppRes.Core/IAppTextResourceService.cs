using System.ComponentModel.DataAnnotations;
using YS.Knife.Query;
using YS.Knife.Service;
using static YS.Knife.AppRes.IAppTextResourceManager;

namespace YS.Knife.AppRes
{
    public interface IAppTextResourceService : IQueryPageApi<AppTextResourceInfo>
    {
        Task<StreamBody> GetContent(string key, CancellationToken cancellationToken);



    }
}

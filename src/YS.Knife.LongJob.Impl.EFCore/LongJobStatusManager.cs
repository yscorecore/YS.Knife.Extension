using Microsoft.EntityFrameworkCore;
using YS.Knife.Entity;
using YS.Knife.LongJob.Entity.EFCore;
using static YS.Knife.LongJob.ILongJobStatusManager;

namespace YS.Knife.LongJob.Impl.EFCore
{
    [Service(typeof(ILongJobStatusManager))]
    [AutoConstructor]
    public partial class LongJobStatusManager : ILongJobStatusManager
    {
        private readonly IEntityStore<LongJobEntity> entityStore;

        public async Task<Guid> CreateNew(string type, string? description = null, CancellationToken token = default)
        {
            var entity = new LongJobEntity
            {
                Type = type,
                Description = description,
            };
            entityStore.Add(entity);
            await entityStore.SaveChangesAsync(token);
            return entity.Id;
        }

        public async Task Update(JobStatus jobStatus, CancellationToken token)
        {
            var entity = await entityStore.Current.FindOrThrowAsync(jobStatus.Id, token);
            entity.Message = jobStatus.Message;
            entity.Percentage = jobStatus.Percentage;
            entity.Completed = jobStatus.Completed;
            entity.Success = jobStatus.Success;
            entity.Assets = jobStatus.Assets;
            await entityStore.SaveChangesAsync(token);
        }
    }
}

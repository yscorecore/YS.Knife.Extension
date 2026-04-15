
using System.Runtime;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using YS.Knife.Entity;
using YS.Knife.Job.Entity.EFCore;
using YS.Knife.Job.Impl.EFCore;

namespace YS.Knife.Job.Impl
{
    [Service]
    [AutoConstructor]
    public partial class JobService : IJobService
    {
        private readonly IEntityStore<JobEntity<Guid>> jobStore;

        private readonly IEnumerable<IJob> allJobs;

        private readonly ILogger<JobService> logger;



        public async Task<bool> EcecOrPush<T>(string type, T data)
        {
            var job = allJobs.Where(p => p.Type == type).OfType<IJob<T>>().FirstOrDefault();
            if (job != null)
            {
                var (Success, ErrorMessage) = await job.ExecuteArgument(data);
                if (Success)
                {
                    return true;
                }
                else
                {
                    return await PushJob(type, data);
                }
            }
            return await PushJob(type, data);
        }

        public async Task<bool> PushJob<T>(string type, params T[] data)
        {
            jobStore.AddRange(data.Select(p => new JobEntity<Guid>
            {
                 Type =type,
                 Argument = JobDataSerializerService.Instance.SerializeToString(p),
            }));
            await jobStore.SaveChangesAsync();
            return true;
        }
    }
}

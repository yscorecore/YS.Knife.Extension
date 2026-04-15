using YS.Knife.Entity;
using YS.Knife.HostedService;

namespace YS.Knife.Job.BackgroundWorker
{
    [HostedService]
    [AutoConstructor]
    public partial class JobWorker : TimerTickHostedService
    {
        private readonly IJobService jobService;
        private readonly IEntityStore<JobInfo> jobStore;
        protected override TimeSpan Interval => TimeSpan.FromSeconds(10);

        protected override void OnException(Exception exception)
        {
          
        }

        protected override void OnTick(CancellationToken state)
        {

        }
    }
}

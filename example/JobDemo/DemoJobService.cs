using Hangfire;
using YS.Knife.Job;

namespace JobDemo
{
    [YS.Knife.Service(typeof(IDemoJobService))]
    public class DemoJobService : IDemoJobService
    {
        private readonly IJobService jobService;
        private string recurringJobId = "HelloJob.SayHi";

        public DemoJobService(IJobService jobService)
        {
            this.jobService = jobService;
        }

        public async Task<string> Enqueue()
        {
            await jobService.Run<HelloJob>(x => x.SayHi("[Default]"), RetryPolicy.Default, JobPriority.Default);
            return "enqueued (Default priority)";
        }

        public async Task<string> Schedule()
        {
            await jobService.RunAfter<HelloJob>(x => x.SayHi("[Default]"), TimeSpan.FromSeconds(5), RetryPolicy.Never, JobPriority.Default);
            return "scheduled after 5 seconds (Default priority)";
        }

        public async Task<string> AddRecurring()
        {
            recurringJobId = await jobService.RunRecurring<HelloJob>(x => x.SayHi("[Default]"), Cron.MinuteInterval(1), RetryPolicy.Never, JobPriority.Default);
            return recurringJobId;
        }

        public async Task<string> RemoveRecurring()
        {
            await jobService.RemoveRecurring(recurringJobId);
            return "removed: " + recurringJobId;
        }

        public async Task<string> EnqueueWithRetry()
        {
            // 自定义重试策略：最多重试 2 次，间隔分别为 2 秒、3 秒
            await jobService.Run<FlakyJob>(x => x.Run(), RetryPolicy.FromDelaySeconds(1, 2, 4, 8, 30), JobPriority.Default);
            return "enqueued flaky job (retry 2 times, delays 2s/3s)";
        }

        public async Task<string> EnqueueWithPriority(JobPriority priority)
        {
            // 把优先级拼到任务标签里，执行时可在控制台看到对应输出
            await jobService.Run<HelloJob>(x => x.SayHi($"[{priority}]"), RetryPolicy.Never, priority);
            return $"enqueued with priority {priority} (queue: {priority.ToString().ToLowerInvariant()})";
        }
    }
}

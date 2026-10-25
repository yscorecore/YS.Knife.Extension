using YS.Knife.Job;

namespace JobDemo
{
    public interface IDemoJobService
    {
        Task<string> Enqueue();

        Task<string> Schedule();

        Task<string> AddRecurring();

        Task<string> RemoveRecurring();

        Task<string> EnqueueWithRetry();

        /// <summary>
        /// 按指定优先级入队一个任务，用于演示 JobPriority 如何映射到 Hangfire 队列。
        /// </summary>
        Task<string> EnqueueWithPriority(JobPriority priority);
    }
}

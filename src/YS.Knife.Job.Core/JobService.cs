using System.Linq.Expressions;

namespace YS.Knife.Job
{
    /// <summary>
    /// Represents the priority of a job. Maps to the underlying scheduler's queue.
    /// </summary>
    public enum JobPriority
    {
        Critical = 0,
        Default = 1,
        Low = 2
    }

    public interface IJobService
    {
        /// <summary>
        /// Enqueues a job to be executed as soon as possible.
        /// </summary>
        Task Run<TService>(Expression<Func<TService, Task>> methodCall, RetryPolicy retryPolicy, JobPriority priority = JobPriority.Default, CancellationToken token = default);
        /// <summary>
        /// Schedules a job to be executed after the specified delay.
        /// </summary>
        Task RunAfter<TService>(Expression<Func<TService, Task>> methodCall, TimeSpan delay, RetryPolicy retryPolicy, JobPriority priority = JobPriority.Default, CancellationToken token = default);
        /// <summary>
        /// Schedules a job to be executed at the specified time.
        /// </summary>
        Task RunAt<TService>(Expression<Func<TService, Task>> methodCall, DateTimeOffset time, RetryPolicy retryPolicy, JobPriority priority = JobPriority.Default, CancellationToken token = default);
        /// <summary>
        /// Creates or updates a recurring job with the specified cron expression.
        /// The job identifier is generated from the service type and the invoked method name.
        /// </summary>
        /// <returns>The recurring job identifier.</returns>
        Task<string> RunRecurring<TService>(Expression<Func<TService, Task>> methodCall, string cronExpression, RetryPolicy retryPolicy, JobPriority priority = JobPriority.Default, CancellationToken token = default);
        /// <summary>
        /// Removes the recurring job with the specified identifier if it exists.
        /// </summary>
        Task RemoveRecurring(string jobId, CancellationToken token = default);
    }

    public class RetryPolicy
    {
        public int MaxRetryCount { get; init; }
        public int[] DelaySeconds { get; init; } = new int[0];

        public static RetryPolicy Never { get; } = new RetryPolicy { MaxRetryCount = 0, DelaySeconds = Array.Empty<int>() };

        public static RetryPolicy Default { get; } = new RetryPolicy { MaxRetryCount = 6, DelaySeconds = new int[] { 5, 30, 180, 900, 3600, 21600 } };
        public static RetryPolicy FromDelaySeconds(params int[] delaySeconds)
        {
            return new RetryPolicy
            {
                MaxRetryCount = delaySeconds.Length,
                DelaySeconds = delaySeconds
            };
        }
    }
}

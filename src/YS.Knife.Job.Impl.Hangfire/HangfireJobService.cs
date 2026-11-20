using System.Linq.Expressions;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Microsoft.Extensions.Logging;
using HangfireJob = Hangfire.Common.Job;

namespace YS.Knife.Job.Impl.Hangfire
{
    [Service(typeof(IJobService))]
    [AutoConstructor]
    [Logger]
    public partial class HangfireJobService : IJobService
    {
        private readonly BackgroundJobClient client;
        private readonly IRecurringJobManager recurringJobManager;
        private readonly JobStorage storage;

        [AutoConstructorInitialize]
        private void RegisterRetryFilter()
        {
            RetryPolicyStateFilter.EnsureRegistered();
        }

        public Task Run<TService>(Expression<Func<TService, Task>> methodCall, RetryPolicy retryPolicy, JobPriority priority = JobPriority.Default, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            var jobId = CreateJob(HangfireJob.FromExpression(methodCall), new EnqueuedState(ToQueueName(priority)), retryPolicy, priority);
            logger.LogDebug("Enqueued job {JobId} with retry policy {@RetryPolicy} and priority {Priority}.", jobId, retryPolicy, priority);
            return Task.CompletedTask;
        }

        public Task RunAfter<TService>(Expression<Func<TService, Task>> methodCall, TimeSpan delay, RetryPolicy retryPolicy, JobPriority priority = JobPriority.Default, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            var jobId = CreateJob(HangfireJob.FromExpression(methodCall), new ScheduledState(delay), retryPolicy, priority);
            logger.LogDebug("Scheduled job {JobId} after {Delay} with retry policy {@RetryPolicy} and priority {Priority}.", jobId, delay, retryPolicy, priority);
            return Task.CompletedTask;
        }

        public Task RunAt<TService>(Expression<Func<TService, Task>> methodCall, DateTimeOffset time, RetryPolicy retryPolicy, JobPriority priority = JobPriority.Default, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            var jobId = CreateJob(HangfireJob.FromExpression(methodCall), new ScheduledState(time.UtcDateTime), retryPolicy, priority);
            logger.LogDebug("Scheduled job {JobId} at {Time} with retry policy {@RetryPolicy} and priority {Priority}.", jobId, time, retryPolicy, priority);
            return Task.CompletedTask;
        }

        public Task<string> RunRecurring<TService>(Expression<Func<TService, Task>> methodCall, string cronExpression, RetryPolicy retryPolicy, JobPriority priority = JobPriority.Default, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            var jobId = GetRecurringJobId<TService>(methodCall);
            // QueueName is obsolete in Hangfire 1.8.25, but the non-obsolete overload with an
            // explicit queue parameter uses Job.Queue, which requires the JobQueueProperty
            // storage feature and throws NotSupportedException with MemoryStorage.
#pragma warning disable CS0618
            recurringJobManager.AddOrUpdate(jobId, methodCall, cronExpression, new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Utc,
                QueueName = ToQueueName(priority)
            });
#pragma warning restore CS0618
            ApplyRecurringRetryPolicy(jobId, retryPolicy);
            logger.LogDebug("Added or updated recurring job {JobId} with cron {CronExpression}, retry policy {@RetryPolicy} and priority {Priority}.", jobId, cronExpression, retryPolicy, priority);
            return Task.FromResult(jobId);
        }

        public Task RemoveRecurring(string jobId, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(jobId))
            {
                throw new ArgumentException("Job id cannot be null or empty.", nameof(jobId));
            }
            recurringJobManager.RemoveIfExists(jobId);
            return Task.CompletedTask;
        }

        private static string GetRecurringJobId<TService>(Expression<Func<TService, Task>> methodCall)
        {
            if (methodCall.Body is not MethodCallExpression call)
            {
                throw new ArgumentException("The expression must be a direct method call, e.g. x => x.ExecuteAsync().", nameof(methodCall));
            }
            return $"{typeof(TService).Name}.{call.Method.Name}";
        }

        private string CreateJob(HangfireJob job, IState state, RetryPolicy retryPolicy, JobPriority priority)
        {
            return client.Create(job, state, BuildParameters(retryPolicy, priority));
        }

        private static string ToQueueName(JobPriority priority)
        {
            return priority.ToString().ToLowerInvariant();
        }

        private static Dictionary<string, object> BuildParameters(RetryPolicy retryPolicy, JobPriority priority)
        {
            var parameters = new Dictionary<string, object>();
            if (retryPolicy != null)
            {
                parameters[RetryPolicyStateFilter.MaxRetryCountParameter] = retryPolicy.MaxRetryCount;
                parameters[RetryPolicyStateFilter.DelaySecondsParameter] = string.Join(',', retryPolicy.DelaySeconds ?? Array.Empty<int>());
            }
            if (priority != JobPriority.Default)
            {
                parameters[RetryPolicyStateFilter.PriorityParameter] = ToQueueName(priority);
            }
            return parameters;
        }

        private void ApplyRecurringRetryPolicy(string jobId, RetryPolicy retryPolicy)
        {
            if (retryPolicy == null)
            {
                return;
            }
            using var connection = storage.GetConnection();
            connection.SetRangeInHash($"recurring-job:{jobId}", new Dictionary<string, string>
            {
                [RetryPolicyStateFilter.MaxRetryCountParameter] = retryPolicy.MaxRetryCount.ToString(),
                [RetryPolicyStateFilter.DelaySecondsParameter] = string.Join(',', retryPolicy.DelaySeconds ?? Array.Empty<int>())
            });
        }
    }
}

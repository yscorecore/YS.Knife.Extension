using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;

namespace YS.Knife.Job.Impl.Hangfire
{
    /// <summary>
    /// Applies the <see cref="RetryPolicy"/> and priority stored in job parameters
    /// while a job moves through the state machine.
    /// </summary>
    internal class RetryPolicyStateFilter : JobFilterAttribute, IElectStateFilter
    {
        public const string MaxRetryCountParameter = "YS_Knife_Job_MaxRetryCount";
        public const string DelaySecondsParameter = "YS_Knife_Job_DelaySeconds";
        public const string RetryAttemptParameter = "YS_Knife_Job_RetryAttempt";
        public const string PriorityParameter = "YS_Knife_Job_Priority";

        private static int registered;

        public static void EnsureRegistered()
        {
            if (Interlocked.Exchange(ref registered, 1) == 0)
            {
                GlobalJobFilters.Filters.Add(new RetryPolicyStateFilter());
            }
        }

        public void OnStateElection(ElectStateContext context)
        {
            if (context.CandidateState is EnqueuedState enqueuedState)
            {
                ApplyQueue(context, enqueuedState);
            }

            if (context.CandidateState is not FailedState failedState)
            {
                return;
            }

            var (maxRetryCount, delaySeconds) = ResolveRetryPolicy(context);
            if (maxRetryCount <= 0)
            {
                return;
            }

            var attempt = context.GetJobParameter<int>(RetryAttemptParameter) + 1;
            if (attempt > maxRetryCount)
            {
                return;
            }

            var delay = GetDelay(delaySeconds, attempt);
            context.SetJobParameter(RetryAttemptParameter, attempt);
            context.CandidateState = new ScheduledState(delay)
            {
                Reason = $"Retry attempt {attempt}/{maxRetryCount} after failure: {failedState.Exception?.Message}"
            };
        }

        private static void ApplyQueue(ElectStateContext context, EnqueuedState enqueuedState)
        {
            var priority = context.GetJobParameter<string>(PriorityParameter);
            if (string.IsNullOrWhiteSpace(priority)
                || string.Equals(priority, enqueuedState.Queue, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            enqueuedState.Queue = priority;
        }

        private static (int MaxRetryCount, int[] DelaySeconds) ResolveRetryPolicy(ElectStateContext context)
        {
            var maxRetryCount = context.GetJobParameter<int>(MaxRetryCountParameter);
            if (maxRetryCount > 0)
            {
                return (maxRetryCount, ParseDelays(context.GetJobParameter<string>(DelaySecondsParameter)));
            }

            var recurringJobId = context.GetJobParameter<string>("RecurringJobId");
            if (!string.IsNullOrEmpty(recurringJobId))
            {
                var hash = context.Storage.GetConnection().GetAllEntriesFromHash($"recurring-job:{recurringJobId}");
                if (hash != null
                    && hash.TryGetValue(MaxRetryCountParameter, out var maxRetryCountValue)
                    && int.TryParse(maxRetryCountValue, out maxRetryCount)
                    && maxRetryCount > 0)
                {
                    var delays = hash.TryGetValue(DelaySecondsParameter, out var delaysValue)
                        ? ParseDelays(delaysValue)
                        : Array.Empty<int>();
                    return (maxRetryCount, delays);
                }
            }

            return (0, Array.Empty<int>());
        }

        private static TimeSpan GetDelay(int[] delaySeconds, int attempt)
        {
            if (delaySeconds.Length == 0)
            {
                return TimeSpan.Zero;
            }
            var seconds = delaySeconds[Math.Min(attempt - 1, delaySeconds.Length - 1)];
            return TimeSpan.FromSeconds(seconds);
        }

        private static int[] ParseDelays(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Array.Empty<int>();
            }
            return value.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => int.TryParse(p.Trim(), out var seconds) ? seconds : 0)
                .ToArray();
        }
    }
}

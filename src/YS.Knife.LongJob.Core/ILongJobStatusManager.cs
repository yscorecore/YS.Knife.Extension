using YS.Knife.Service;
using static YS.Knife.LongJob.ILongJobStatusManager;

namespace YS.Knife.LongJob
{
    public interface ILongJobStatusManager
    {
        Task Update(Guid id, JobStatus jobStatus, CancellationToken token);

        Task<Guid> CreateNew(string type, string? description = null, CancellationToken token = default);

        public record JobStatus : IdDto<Guid>
        {
            public string? Message { get; set; }
            public double Percentage { get; set; }
            public bool Completed { get; set; }

            public bool Success { get; set; }
            public object? Assets { get; set; }
        }
    }
    public static class LongJobStatusManagerExtensions
    {
        public static Task Success(this ILongJobStatusManager jobStatusManager, Guid id, object? assets, CancellationToken token = default)
        {
            return jobStatusManager.Update(id, new JobStatus
            {
                Id = id,
                Completed = true,
                Percentage = 100,
                Assets = assets,
                Success = true,
            }, token);
        }
        public static Task Fail(this ILongJobStatusManager jobStatusManager, Guid id, string message, CancellationToken token = default)
        {
            return jobStatusManager.Update(id, new JobStatus
            {
                Id = id,
                Completed = true,
                Percentage = 100,
                Message = message,
                Assets = null,
                Success = false,
            }, token);
        }

        public static Task ReportProgress(this ILongJobStatusManager jobStatusManager, Guid id, string message, double percentage, CancellationToken token = default)
        {
            return jobStatusManager.Update(id, new JobStatus
            {
                Id = id,
                Completed = false,
                Percentage = percentage,
                Message = message,
                Assets = null,
                Success = false,
            }, token);
        }
    }


}

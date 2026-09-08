using System.Text.Json;

namespace YS.Knife.Task
{
    public interface ITaskService
    {
        Task<TaskExecuteResult> Execute(TaskInfo taskInfo, CancellationToken cancellationToken = default);
    }
    public static class TaskServiceExtensions
    {
        public static async Task<TaskExecuteResult> Execute(this ITaskService service, string name, string? version, object argument, CancellationToken cancellationToken = default)
        {
            return await service.Execute(new TaskInfo()
            {
                Name = name,
                Version = version,
                Argument = argument
            }, cancellationToken);
        }
    }
}

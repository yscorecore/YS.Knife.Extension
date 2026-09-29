using System.Text.Json;

namespace YS.Knife.Task
{
    public interface ITaskExecutor
    {
        Task<TaskExecuteResult> Execute(TaskInfo taskInfo, CancellationToken cancellationToken = default);


    }
    public static class TaskServiceExtensions
    {
        public static async Task<TaskExecuteResult> Execute(this ITaskExecutor service, string name, string? version, object argument, CancellationToken cancellationToken = default)
        {
            return await service.Execute(new TaskInfo()
            {
                Name = name,
                Version = version,
                Argument = argument
            }, cancellationToken);
        }
        public static async Task<TaskExecuteResult> Execute<TBaseTask, TArg>(this ITaskExecutor service, TArg argument, CancellationToken cancellationToken = default)
            where TArg : new()
            where TBaseTask : BaseTask<TArg>
        {
            var attr = TaskAttribute.GetFromType(typeof(TBaseTask));
            return await service.Execute(attr.Name, attr.Version, argument!, cancellationToken);
        }
    }
}

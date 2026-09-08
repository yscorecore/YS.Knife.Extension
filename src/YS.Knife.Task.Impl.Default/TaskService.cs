using System.Text.Json;

namespace YS.Knife.Task
{
    [Service]
    [AutoConstructor]
    public partial class TaskService : ITaskService
    {
        private readonly IEnumerable<ITask> allTasks;
        [AutoConstructorIgnore]
        private IDictionary<string, ITask> tasks;
        [AutoConstructorInitialize]
        private void Init()
        {
            var lookups = allTasks.ToLookup(p =>
            {
                var attr = TaskAttribute.GetFromType(p.GetType());
                return $"{attr.Name}@{attr.Version}";
            });
            var duplicate = lookups.Where(p => p.Count() > 1).FirstOrDefault();
            if (duplicate != null)
            {
                throw new Exception($"Duplicate task code '{duplicate.Key}'.");
            }

            tasks = lookups.ToDictionary(p => p.Key, p => p.First());
        }
        private async Task<TaskExecuteResult> Execute(string name, string? version, object args, CancellationToken cancellationToken)
        {
            var key = $"{name}@{version}";
            if (tasks.TryGetValue(key, out var task))
            {
                try
                {
                    return await task.ExecuteAsync(args, cancellationToken);
                }
                catch (Exception ex)
                {

                    return TaskExecuteResult.Failure($"Task {key} execution failed: {ex.Message}");
                }

            }
            return TaskExecuteResult.Failure($"Task {key} not found");

        }

        public Task<TaskExecuteResult> Execute(TaskInfo taskInfo, CancellationToken cancellationToken = default)
        {
            return Execute(taskInfo.Name, taskInfo.Version, taskInfo.Argument, cancellationToken);
        }
    }


}

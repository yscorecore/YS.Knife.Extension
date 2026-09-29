using System.Reflection;
using YS.Knife.Metadata;
using YS.Knife.Query;

namespace YS.Knife.Task
{
    [Service(typeof(ITaskService))]
    [Service(typeof(ITaskExecutor))]
    [AutoConstructor]

    public partial class TaskService : ITaskExecutor, ITaskService
    {
        private readonly IEnumerable<ITask> allTasks;
        [AutoConstructorIgnore]
        private IDictionary<string, (ITask, TaskDescription)> tasks;
        [AutoConstructorInitialize]
        private void Init()
        {
            var lookups = allTasks.Select(p => (p, CreateDescriptionFromType(p.GetType()))).ToLookup(p =>
            {
                var desc = p.Item2;
                return $"{desc.Name}@{desc.Version}";
            });
            var duplicate = lookups.Where(p => p.Count() > 1).FirstOrDefault();
            if (duplicate != null)
            {
                throw new Exception($"Duplicate task code '{duplicate.Key}'.");
            }

            tasks = lookups.ToDictionary(p => p.Key, p => p.First());
        }
        private TaskDescription CreateDescriptionFromType(Type type)
        {
            var attr = TaskAttribute.GetFromType(type);
            var argType = GetArgumentType(type);
            var argumentMeta = argType == null ? null : (argType?.GetCustomAttribute<MetadataAttribute>()?.Name ?? argType?.FullName);
            return new TaskDescription
            {
                Name = attr.Name,
                Version = attr.Version,
                Description = attr.Description,
                Group = attr.Group,
                ArgumentMeta = argumentMeta
            };
        }

        private Type? GetArgumentType(Type type)
        {
            var baseType = type.BaseType;

            if (baseType != null && baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(BaseTask<>))
            {
                var argType = baseType.GetGenericArguments().Single();
                return argType;
            }
            return null;
        }
        private async Task<TaskExecuteResult> Execute(string name, string? version, object args, CancellationToken cancellationToken)
        {
            var key = $"{name}@{version}";
            if (tasks.TryGetValue(key, out var task))
            {
                try
                {
                    return await task.Item1.ExecuteAsync(args, cancellationToken);
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

        public Task<PagedList<TaskDescription>> QueryPagedList(LimitQueryInfo req, CancellationToken cancellationToken = default)
        {
            return System.Threading.Tasks.Task.FromResult(tasks.Values.Select(p => p.Item2).AsQueryable().QueryPage(req));
        }


    }


}

namespace YS.Knife.Task
{
    public abstract class BaseTask<TArgs> : ITask<TArgs> where TArgs : new()
    {
        public abstract Task<TaskExecuteResult> ExecuteAsync(TArgs args, CancellationToken cancellationToken = default);


        public Task<TaskExecuteResult> ExecuteAsync(object args, CancellationToken cancellationToken = default)
        {
            return ExecuteAsync(args.AsJsonObject<TArgs>(TaskInfo.JsonSerializerOptions), cancellationToken);
        }
    }
}

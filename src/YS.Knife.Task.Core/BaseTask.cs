namespace YS.Knife.Tasks
{
    public abstract class BaseTask<TArgs> : ITask<TArgs>
    {
        public abstract Task ExecuteAsync(TArgs args, CancellationToken cancellationToken = default);


        public async Task ExecuteAsync(object args, CancellationToken cancellationToken = default)
        {
            await ExecuteAsync(args.AsJsonObject<TArgs>(TaskInfo.JsonSerializerOptions), cancellationToken);
        }
    }
}

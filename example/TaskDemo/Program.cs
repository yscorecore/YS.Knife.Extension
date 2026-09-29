
using YS.Knife.Metadata;
using YS.Knife.Task;

namespace TaskDemo
{
    [YS.Knife.ExposeApi(typeof(IMetadataService))]
    [YS.Knife.ExposeApi(typeof(ITaskExecutor))]
    [YS.Knife.ExposeApi(typeof(ITaskService))]
    public class Program : YS.Knife.Hosting.KnifeWebHost
    {
        public Program(string[] args) : base(args)
        {
        }
        public static void Main(string[] args)
        {
            new Program(args).Run();
        }
    }
    [Task("hello", "测试", version: "v1")]
    [Logger]
    [AutoConstructor]
    public partial class HelloTask : BaseTask<HelloTaskReq>
    {
        public override async Task<TaskExecuteResult> ExecuteAsync(HelloTaskReq args, CancellationToken cancellationToken = default)
        {
            await Task.Delay(1, cancellationToken);
            this.logger.LogInformation("Hello {message}", args.Message);
            return TaskExecuteResult.Ok;

        }
    }

    public record HelloTaskReq
    {
        public string Message { get; set; } = null!;
    }
}

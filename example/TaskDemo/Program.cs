using YS.Knife.Hosting;
using YS.Knife.Metadata;
using YS.Knife.Tasks;

namespace TaskDemo
{
    [YS.Knife.ExposeApi(typeof(IMetadataService))]
    [YS.Knife.ExposeApi(typeof(ITaskExecutor))]
    [YS.Knife.ExposeApi(typeof(ITaskService))]
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddAllKnifeServices(builder.Configuration);
            builder.Services.AddControllers();


            builder.Services.AddSwaggerGen();
            var app = builder.Build();
            app.MapControllers();
            app.UseSwagger();
            app.UseSwaggerUI();

            app.Run();
        }
    }

    [Task("hello", "测试", version: "v1")]
    [Logger]
    [AutoConstructor]
    public partial class HelloTask : BaseTask<HelloTaskReq>
    {
        public override async Task ExecuteAsync(HelloTaskReq args, CancellationToken cancellationToken = default)
        {
            await Task.Delay(1, cancellationToken);
            this.logger.LogInformation("Hello {message}", args.Message);
        }
    }

    public record HelloTaskReq
    {
        public string Message { get; set; } = null!;
    }
}

using System.Text.Json.Serialization;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace JobDemo
{
    [YS.Knife.ExposeApi(typeof(IDemoJobService))]
    public class Program : YS.Knife.Hosting.KnifeWebHost
    {
        private static readonly MemoryStorage Storage = new();

        public Program(string[] args) : base(args)
        {
        }

        public static void Main(string[] args)
        {
            // 只消费 critical / default 两个队列，low 优先级的任务会一直停留在 low 队列中不被执行，
            // 借此直观体现 JobPriority -> Queue 的映射效果。
            using var server = new BackgroundJobServer(new BackgroundJobServerOptions
            {
                SchedulePollingInterval = TimeSpan.FromSeconds(1),
                Queues = new[] { "critical", "default" },
            }, Storage);

            new Program(args).Run();
        }

        protected override void OnConfigureCustomService(HostBuilderContext builder, IServiceCollection serviceCollection)
        {
            base.OnConfigureCustomService(builder, serviceCollection);
            serviceCollection.AddSingleton<JobStorage>(Storage);
            serviceCollection.AddSingleton<HelloJob>();
            // 让 JobPriority 等枚举在请求/响应中使用字符串名称（如 "Critical"），便于演示。
            serviceCollection.Configure<JsonOptions>(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
        }
    }
}

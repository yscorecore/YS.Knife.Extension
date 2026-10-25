# YS.Knife.Job.Impl.Hangfire

基于 Hangfire 的 `IJobService` 实现，将任务调度能力接入 Hangfire 的存储与执行引擎。

## 主要功能

- 实现 `YS.Knife.Job.Core` 定义的 `IJobService` 接口
- 立即/延迟/定时任务通过 `BackgroundJobClient` 创建，并把重试策略与优先级一并写入作业参数
- 周期任务通过 `IRecurringJobManager` 管理，任务 ID 由 `服务类型名.方法名` 自动生成
- 重试策略：注册全局 `IElectStateFilter`（`RetryPolicyStateFilter`），在失败时按策略重新调度，无需在业务方法上打特性
- 优先级：`JobPriority` 映射为 Hangfire 队列（`critical` / `default` / `low`），立即任务直接指定 `EnqueuedState.Queue`，延迟与重试任务在入队时由 filter 应用优先级队列
- 自动注册 `HangfireJobService`、`IBackgroundJobClient`、`IRecurringJobManager`，并将宿主提供的 `JobStorage` 注入

## 快速开始

1. 引用本包与 Hangfire 的存储实现（如 `Hangfire.MemoryStorage`、`Hangfire.SqlServer` 等）。
2. 注册 Hangfire 存储与服务器，并把 `JobStorage` 交给 DI：

```csharp
var storage = new MemoryStorage();
services.AddSingleton<JobStorage>(storage);
// 启动后台作业服务器（按需配置队列与轮询间隔）
services.AddHangfireServer(options =>
{
    options.Queues = new[] { "critical", "default" };
    options.SchedulePollingInterval = TimeSpan.FromSeconds(1);
}, storage);
```

> `HangfireJobService` 会通过 DI 获取已注册的 `JobStorage`；若未显式注册，则回退到 `JobStorage.Current`。

3. 注入 `IJobService` 使用：

```csharp
public class MyService
{
    private readonly IJobService jobService;

    public MyService(IJobService jobService)
    {
        this.jobService = jobService;
    }

    public async Task Demo()
    {
        // 立即执行，失败重试 3 次
        await jobService.Run<IEmailJob>(x => x.Send(), RetryPolicy.FromDelaySeconds(5, 30, 60));

        // 延迟 10 秒，低优先级
        await jobService.RunAfter<IEmailJob>(x => x.Send(), TimeSpan.FromSeconds(10), RetryPolicy.Never, JobPriority.Low);

        // 每分钟执行一次的周期任务
        var jobId = await jobService.RunRecurring<IReportJob>(x => x.Daily(), "* * * * *", RetryPolicy.Default);
    }
}
```

## 优先级与队列

`JobPriority` 被映射为 Hangfire 队列名（小写）：

| JobPriority | 队列名    |
| ----------- | --------- |
| Critical    | critical  |
| Default     | default   |
| Low         | low       |

要让优先级真正生效，需要在启动 `BackgroundJobServer` 时配置 `Queues`，并按业务需要消费相应队列。例如只消费高优先级队列：

```csharp
services.AddHangfireServer(options =>
{
    options.Queues = new[] { "critical" };
});
```

## 重试策略

重试次数与间隔在创建任务时一次性写入作业参数，由全局 filter 读取并执行：

- `RetryPolicy.Never`：不重试
- `RetryPolicy.Default`：最多 6 次，间隔 5s/30s/3min/15min/1h/6h
- `RetryPolicy.FromDelaySeconds(5, 30, 60)`：重试 3 次，间隔 5s/30s/60s

## 依赖说明

- `YS.Knife.Job.Core`：提供 `IJobService`、`RetryPolicy`、`JobPriority`
- `YS.Knife.Core`：提供 `[Service]` / `IServiceRegister` 自动注册能力
- `Hangfire.Core`（1.8.*）：任务调度核心
- Hangfire 存储实现（如 `Hangfire.MemoryStorage`、`Hangfire.SqlServer`），由宿主项目引用

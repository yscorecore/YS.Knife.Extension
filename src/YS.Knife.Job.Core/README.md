# YS.Knife.Job.Core

异步任务调度的抽象层，定义统一的任务服务接口 `IJobService`，支持立即执行、延迟执行、定时执行、周期任务，以及重试策略和优先级。

## 主要功能

- 定义 `IJobService` 接口，统一抽象四类任务调度方式：
  - `Run`：立即入队执行
  - `RunAfter`：延迟指定时间后执行
  - `RunAt`：在指定时间点执行
  - `RunRecurring`：按 Cron 表达式周期执行，自动生成任务 ID
  - `RemoveRecurring`：移除周期任务
- 定义 `RetryPolicy` 重试策略，支持预设策略与自定义重试间隔
- 定义 `JobPriority` 任务优先级，映射到调度器的队列

## 接口定义

```csharp
public interface IJobService
{
    Task Run<TService>(Expression<Func<TService, Task>> methodCall, RetryPolicy retryPolicy, JobPriority priority = JobPriority.Default, CancellationToken token = default);
    Task RunAfter<TService>(Expression<Func<TService, Task>> methodCall, TimeSpan delay, RetryPolicy retryPolicy, JobPriority priority = JobPriority.Default, CancellationToken token = default);
    Task RunAt<TService>(Expression<Func<TService, Task>> methodCall, DateTimeOffset time, RetryPolicy retryPolicy, JobPriority priority = JobPriority.Default, CancellationToken token = default);
    Task<string> RunRecurring<TService>(Expression<Func<TService, Task>> methodCall, string cronExpression, RetryPolicy retryPolicy, JobPriority priority = JobPriority.Default, CancellationToken token = default);
    Task RemoveRecurring(string jobId, CancellationToken token = default);
}
```

### 任务优先级

`JobPriority` 枚举定义了三个优先级，对应调度器中的不同队列：

| 枚举值   | 队列名    | 说明     |
| -------- | --------- | -------- |
| Critical | critical  | 最高优先级 |
| Default  | default   | 默认优先级 |
| Low      | low       | 最低优先级 |

### 重试策略

`RetryPolicy` 提供以下预设：

- `RetryPolicy.Never`：不重试
- `RetryPolicy.Default`：最多重试 6 次，间隔依次为 5s、30s、3min、15min、1h、6h
- `RetryPolicy.FromDelaySeconds(params int[] delaySeconds)`：自定义重试次数与间隔

## 使用示例

```csharp
public class OrderService
{
    private readonly IJobService jobService;

    public OrderService(IJobService jobService)
    {
        this.jobService = jobService;
    }

    public async Task PlaceOrder()
    {
        // 立即执行，失败重试 3 次，间隔 5s/10s/30s
        await jobService.Run<INotifyService>(x => x.SendEmail(),
            RetryPolicy.FromDelaySeconds(5, 10, 30));

        // 延迟 1 小时执行，最高优先级
        await jobService.RunAfter<ICouponService>(x => x.Expire(),
            TimeSpan.FromHours(1), RetryPolicy.Never, JobPriority.Critical);

        // 每天 0 点执行的周期任务（返回值为任务 ID）
        var jobId = await jobService.RunRecurring<IReportService>(x => x.Daily(),
            "0 0 * * *", RetryPolicy.Default);
    }
}
```

## 依赖说明

本包仅为抽象层，无第三方依赖。需要配合具体实现使用：

- `YS.Knife.Job.Impl.Hangfire`：基于 Hangfire 的实现

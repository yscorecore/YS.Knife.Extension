using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using YS.Knife.Hosting;

namespace YS.Knife.Job.Impl.Hangfire.UnitTest
{
    public class HangfireJobServiceTest : KnifeHost
    {
        private readonly JobStorage storage = new MemoryStorage();
        private readonly IJobService jobService;

        public HangfireJobServiceTest()
        {
            jobService = this.GetService<IJobService>();
        }

        protected override void OnConfigureCustomService(HostBuilderContext builder, IServiceCollection serviceCollection)
        {
            base.OnConfigureCustomService(builder, serviceCollection);
            serviceCollection.AddSingleton(storage);
        }

        [Fact]
        public void Should_Resolve_IJobService()
        {
            jobService.Should().NotBeNull();
        }

        [Fact]
        public void Should_Resolve_Hangfire_Clients()
        {
            this.GetService<IBackgroundJobClient>().Should().NotBeNull();
            this.GetService<IRecurringJobManager>().Should().NotBeNull();
        }

        [Fact]
        public async Task Should_Generate_Recurring_Job_Id_From_Service_And_Method()
        {
            var jobId = await jobService.RunRecurring<HelloService>(x => x.SayHello(), "* * * * *", RetryPolicy.Default);
            jobId.Should().Be("HelloService.SayHello");
            await jobService.RemoveRecurring(jobId);
        }

        [Fact]
        public void Should_Create_Never_RetryPolicy()
        {
            RetryPolicy.Never.MaxRetryCount.Should().Be(0);
            RetryPolicy.Never.DelaySeconds.Should().BeEmpty();
        }

        [Fact]
        public void Should_Create_Default_RetryPolicy()
        {
            RetryPolicy.Default.MaxRetryCount.Should().Be(6);
            RetryPolicy.Default.DelaySeconds.Should().Equal(5, 30, 180, 900, 3600, 21600);
        }

        [Fact]
        public void Should_Create_RetryPolicy_From_Delay_Seconds()
        {
            var policy = RetryPolicy.FromDelaySeconds(1, 2, 3);
            policy.MaxRetryCount.Should().Be(3);
            policy.DelaySeconds.Should().Equal(1, 2, 3);
        }

        [Fact]
        public async Task Should_Retry_Failed_Job_Until_Success()
        {
            GlobalJobFilters.Filters.Count.Should().BeGreaterThan(0, "RetryPolicyStateFilter should have been registered to GlobalJobFilters");
            FlakyTestService.Reset();
            using var server = new BackgroundJobServer(new BackgroundJobServerOptions
            {
                SchedulePollingInterval = TimeSpan.FromSeconds(1),
            }, storage);

            await jobService.Run<FlakyTestService>(x => x.Run(), RetryPolicy.FromDelaySeconds(1, 1));

            var deadline = DateTime.UtcNow.AddSeconds(20);
            while (FlakyTestService.Attempts < 3 && DateTime.UtcNow < deadline)
            {
                await Task.Delay(200);
            }

            FlakyTestService.Attempts.Should().BeGreaterOrEqualTo(3, $"FlakyTestService should have retried at least 3 times, but ran {FlakyTestService.Attempts} times");
        }

        [Fact]
        public void Should_Expose_Three_Priority_Levels()
        {
            Enum.GetValues<JobPriority>().Should().Equal(JobPriority.Critical, JobPriority.Default, JobPriority.Low);
            JobPriority.Critical.ToString().ToLowerInvariant().Should().Be("critical");
            JobPriority.Default.ToString().ToLowerInvariant().Should().Be("default");
            JobPriority.Low.ToString().ToLowerInvariant().Should().Be("low");
        }

        [Theory]
        [InlineData(JobPriority.Critical, "critical")]
        [InlineData(JobPriority.Default, "default")]
        [InlineData(JobPriority.Low, "low")]
        public async Task Should_Enqueue_Immediate_Job_To_Correct_Queue(JobPriority priority, string expectedQueue)
        {
            await jobService.Run<HelloService>(x => x.SayHello(), RetryPolicy.Never, priority);

            storage.GetMonitoringApi().EnqueuedJobs(expectedQueue, 0, 100).Count.Should().Be(1);
        }

        [Fact]
        public async Task Should_Apply_Priority_To_Delayed_Job_When_Enqueued()
        {
            using var server = new BackgroundJobServer(new BackgroundJobServerOptions
            {
                SchedulePollingInterval = TimeSpan.FromMilliseconds(500),
                Queues = new[] { "critical" },
            }, storage);

            await jobService.RunAfter<HelloService>(x => x.SayHello(), TimeSpan.FromMilliseconds(300), RetryPolicy.Never, JobPriority.Low);

            var deadline = DateTime.UtcNow.AddSeconds(10);
            var count = 0;
            while ((count = storage.GetMonitoringApi().EnqueuedJobs("low", 0, 100).Count) == 0 && DateTime.UtcNow < deadline)
            {
                await Task.Delay(200);
            }

            count.Should().Be(1);
        }

        [Fact]
        public async Task Should_Enqueue_Retried_Job_To_Priority_Queue()
        {
            FlakyTestService.Reset();
            using var server = new BackgroundJobServer(new BackgroundJobServerOptions
            {
                SchedulePollingInterval = TimeSpan.FromMilliseconds(500),
                Queues = new[] { "default" },
            }, storage);

            await jobService.Run<FlakyTestService>(x => x.Run(), RetryPolicy.FromDelaySeconds(1), JobPriority.Low);

            var deadline = DateTime.UtcNow.AddSeconds(10);
            var count = 0;
            while ((count = storage.GetMonitoringApi().EnqueuedJobs("low", 0, 100).Count) == 0 && DateTime.UtcNow < deadline)
            {
                await Task.Delay(200);
            }

            count.Should().Be(1, "the retried job should be re-enqueued to the low priority queue");
        }

        [Fact]
        public async Task Should_Set_Queue_For_Recurring_Job_By_Priority()
        {
            var jobId = await jobService.RunRecurring<HelloService>(x => x.SayHello(), "* * * * *", RetryPolicy.Never, JobPriority.Critical);
            try
            {
                var entries = storage.GetConnection().GetAllEntriesFromHash($"recurring-job:{jobId}");
                entries.Should().ContainKey("Queue");
                entries["Queue"].Should().Be("critical");
            }
            finally
            {
                await jobService.RemoveRecurring(jobId);
            }
        }

        public class HelloService
        {
            public Task SayHello() => Task.CompletedTask;
        }

        public class FlakyTestService
        {
            private static int attempt;
            public static int Attempts => attempt;
            public static void Reset() => attempt = 0;

            public Task Run()
            {
                attempt++;
                if (attempt < 3)
                {
                    throw new InvalidOperationException($"Simulated failure #{attempt}");
                }
                return Task.CompletedTask;
            }
        }
    }
}

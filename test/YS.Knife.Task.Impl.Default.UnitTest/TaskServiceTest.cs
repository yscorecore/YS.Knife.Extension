using YS.Knife.Hosting;
using YS.Knife.Metadata;
using YS.Knife.Query;
using YS.Knife.Tasks;

namespace YS.TaskImplDefault.UnitTest
{
    public class TaskServiceTest : KnifeHost
    {
        private readonly ITaskService taskService;
        private readonly ITaskExecutor taskExecutor;

        public TaskServiceTest()
        {
            taskService = this.GetService<ITaskService>();
            taskExecutor = this.GetService<ITaskExecutor>();
        }

        private async Task<TaskDescription> GetDescription(string name)
        {
            var page = await taskService.QueryPagedList(new LimitQueryInfo { Limit = 1000 });
            return page.Items.Single(p => p.Name == name);
        }

        [Fact]
        public async Task Should_Mark_IsArray_False_For_Plain_Argument()
        {
            var desc = await GetDescription("plain-arg-task");
            desc.IsArray.Should().BeFalse();
            desc.ArgumentMeta.Should().Be("System.String");
        }

        [Fact]
        public async Task Should_Mark_IsArray_True_For_Array_Argument()
        {
            var desc = await GetDescription("array-arg-task");
            desc.IsArray.Should().BeTrue();
            desc.ArgumentMeta.Should().Be("System.Int32");
        }

        [Fact]
        public async Task Should_Mark_IsArray_True_For_List_Argument()
        {
            var desc = await GetDescription("list-arg-task");
            desc.IsArray.Should().BeTrue();
            desc.ArgumentMeta.Should().Be("System.String");
        }

        [Fact]
        public async Task Should_Mark_IsArray_True_For_Enumerable_Argument()
        {
            var desc = await GetDescription("enumerable-arg-task");
            desc.IsArray.Should().BeTrue();
            desc.ArgumentMeta.Should().Be("System.Int32");
        }

        [Fact]
        public async Task Should_Use_Element_Type_Meta_For_Array_Argument()
        {
            var desc = await GetDescription("custom-array-task");
            desc.IsArray.Should().BeTrue();
            desc.ArgumentMeta.Should().Be("CustomArgMeta");
        }

        [Fact]
        public async Task Should_Execute_Task_With_Array_Argument()
        {
            var result = await taskExecutor.Execute(new TaskInfo
            {
                Name = "array-arg-task",
                Argument = new[] { 1, 2, 3 }
            });
            result.Success.Should().BeTrue();
        }

        [Fact]
        public async Task Should_Execute_Task_With_List_Argument()
        {
            var result = await taskExecutor.Execute(new TaskInfo
            {
                Name = "list-arg-task",
                Argument = new List<string> { "a", "b" }
            });
            result.Success.Should().BeTrue();
        }

        [Fact]
        public async Task Should_Build_TaskInfo_From_Array_Object()
        {
            var info = TaskInfo.FromObject(new[] { 1, 2, 3 }, "array-arg-task", null);
            var result = await taskExecutor.Execute(info);
            result.Success.Should().BeTrue();
        }

        [Task("plain-arg-task", "plain argument task")]
        public class PlainArgTask : BaseTask<string>
        {
            public override Task ExecuteAsync(string args, CancellationToken cancellationToken = default)
                => Task.CompletedTask;
        }

        [Task("array-arg-task", "array argument task")]
        public class ArrayArgTask : BaseTask<int[]>
        {
            public override Task ExecuteAsync(int[] args, CancellationToken cancellationToken = default)
                => Task.CompletedTask;
        }

        [Task("list-arg-task", "list argument task")]
        public class ListArgTask : BaseTask<List<string>>
        {
            public override Task ExecuteAsync(List<string> args, CancellationToken cancellationToken = default)
                => Task.CompletedTask;
        }

        [Task("enumerable-arg-task", "enumerable argument task")]
        public class EnumerableArgTask : BaseTask<IEnumerable<int>>
        {
            public override Task ExecuteAsync(IEnumerable<int> args, CancellationToken cancellationToken = default)
                => Task.CompletedTask;
        }

        [Task("custom-array-task", "custom array argument task")]
        public class CustomArrayArgTask : BaseTask<CustomArg[]>
        {
            public override Task ExecuteAsync(CustomArg[] args, CancellationToken cancellationToken = default)
                => Task.CompletedTask;
        }

        [Metadata("CustomArgMeta")]
        public class CustomArg
        {
            public string Name { get; set; }
        }
    }
}

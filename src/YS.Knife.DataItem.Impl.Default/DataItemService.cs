

using Microsoft.Extensions.DependencyInjection;

namespace YS.Knife.DataSource.Impl.Default
{
    [Service]
    [AutoConstructor]
    public partial class DataItemService : IDataItemService
    {
        private readonly IServiceProvider serviceProvider;

        public async Task<object> GetItem(string name, object[] args, CancellationToken cancellationToken)
        {
            var entry = GetDataItemEntry(name);
            var serviceInstance = serviceProvider.GetRequiredService(entry.ServiceType);
            return await InvokeMethod(entry, serviceInstance, args, cancellationToken);
        }
        private Task<object> InvokeMethod(DataItemEntry entry, object instance, object[] args, CancellationToken token)
        {
            ITaskInvoker invoker = Activator.CreateInstance(typeof(TaskInvoker<>).MakeGenericType(entry.ReturnType)) as ITaskInvoker;
            if (entry.IsValueTask)
            {
                return invoker.InvokeValueTaskMethod(entry, instance, args, token);
            }
            else
            {
                return invoker.InvokeTaskMethod(entry, instance, args, token);
            }
        }
        private DataItemEntry GetDataItemEntry(string name)
        {
            return AssemblyDataItemEntryFinder.Instance.All.TryGetValue(name, out var entry) ? entry : throw new Exception($"can not find data item '{name}'");
        }

        public Task<List<DataItemDesc>> GetAllDataItems()
        {
            var res = AssemblyDataItemEntryFinder.Instance.All
                 .Select(p => new DataItemDesc
                 {
                     Name = p.Key,
                     Description = p.Key
                 }).ToList();
            return Task.FromResult(res);
        }

        public Task<DataItemEntry> GetEntry(string name)
        {
            return Task.FromResult(AssemblyDataItemEntryFinder.Instance.All[name]);
        }

        interface ITaskInvoker
        {
            Task<object> InvokeValueTaskMethod(DataItemEntry method, object service, object[] args, CancellationToken token);
            Task<object> InvokeTaskMethod(DataItemEntry method, object service, object[] args, CancellationToken token);
        }
        class TaskInvoker<T> : ITaskInvoker
        {
            public async Task<object> InvokeTaskMethod(DataItemEntry method, object service, object[] args, CancellationToken token)
            {
                var task = (Task<T>)(method.Method.Invoke(service, CombinArguments(method, args, token)));
                var res = await task;
                return res;
            }

            public async Task<object> InvokeValueTaskMethod(DataItemEntry method, object service, object[] args, CancellationToken token)
            {
                var task = (ValueTask<T>)(method.Method.Invoke(service, CombinArguments(method, args, token)));
                var res = await task;
                return res;
            }
            private object[] CombinArguments(DataItemEntry entry, object[] args, CancellationToken token)
            {
                return entry.HasCancellationToken ?
                    (args).Concat(new object[] { token }).ToArray()
                    : args;

            }
        }

    }
}

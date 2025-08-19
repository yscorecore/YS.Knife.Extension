

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

namespace YS.Knife.DataItem.Impl.Default
{
    [Service]
    [AutoConstructor]
    public partial class DataItemService : IDataItemService
    {
        private readonly IServiceProvider serviceProvider;


        private async Task<object> GetItemObject(string name, IDictionary<string, StringValues> arguments, CancellationToken cancellationToken)
        {
            var entry = GetDataItemEntry(name);
            var serviceInstance = serviceProvider.GetRequiredService(entry.ServiceType);

            return await InvokeMethod(entry, serviceInstance, Array.Empty<object>(), cancellationToken); ;
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
        public async Task<Dictionary<string, object>> GetItems(string[] names, DataItemArguments arguments, CancellationToken cancellationToken)
        {
            var dic = new Dictionary<string, object>();
            foreach (var name in (names ?? Array.Empty<string>()).Distinct())
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                dic[name] = await GetItemObject(name, arguments.GetItemArguments(name), cancellationToken);
            }
            return dic;
        }

        public Task<T> GetItem<T, TArg>(string name, TArg arg, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
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
                    (args ).Concat(new object[] { token }).ToArray()
                    : args ;

            }
        }

    }
}

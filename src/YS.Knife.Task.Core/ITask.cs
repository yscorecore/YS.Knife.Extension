using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace YS.Knife.Task
{
    public interface ITask
    {
        Task<TaskExecuteResult> ExecuteAsync(object args, CancellationToken cancellationToken = default);
    }

    public interface ITask<TArgs> : ITask
        where TArgs : new()
    {
        Task<TaskExecuteResult> ExecuteAsync(TArgs args, CancellationToken cancellationToken = default);
    }
}

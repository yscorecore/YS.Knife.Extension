using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace YS.Knife.Tasks
{
    public interface ITask
    {
        Task ExecuteAsync(object args, CancellationToken cancellationToken = default);
    }

    public interface ITask<TArgs> : ITask
    {
        Task ExecuteAsync(TArgs args, CancellationToken cancellationToken = default);
    }
}

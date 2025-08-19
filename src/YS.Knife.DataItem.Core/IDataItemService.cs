using System.Net.Http.Headers;
using Microsoft.Extensions.Primitives;

namespace YS.Knife.DataItem
{
    public interface IDataItemService
    {
        Task<T> GetItem<T, TArg>(string name, TArg arg, CancellationToken cancellationToken);

        Task<Dictionary<string, object>> GetItems(string[] names, DataItemArguments arguments, CancellationToken cancellationToken);
    }
    public record DataItemArguments
    {
        public Dictionary<string,StringValues> CommonArguments { get; set; }

        public Dictionary<string, Dictionary<string, StringValues>> ItemArguments { get; set; }

        public Dictionary<string, StringValues> GetItemArguments(string name)
        {
            if (ItemArguments.TryGetValue(name, out var values))
            {
                var res = new Dictionary<string, StringValues>(CommonArguments);
                foreach (var (k, v) in values)
                {
                    res[k] = v;
                }
                return res;
            }
            else
            {
                return CommonArguments;
            }
        }

    }
}

using System.Runtime.CompilerServices;
using Microsoft.Extensions.Options;

namespace YS.Knife.Mapper.Impl.FlyTiger
{
    [AutoConstructor]
    public partial class FlytigerMapper : IQuerableMapper, IConvertMapper, ICopyMapper
    {
        private readonly IOptions<FlyTigerMapperOptions> options;
        public IQueryable<T> MapQuery<From, T>(IQueryable<From> source) where T : new()
        {
            if (options.Value.QueryableFuncs.TryGetValue((typeof(From), typeof(T)), out var fun))
            {
                var fun1 = (Func<IQueryable<From>, IQueryable<T>>)fun;
                return fun1(source);
            }
            else
            {
                throw new Exception("Can not find query mapper from " + typeof(From).FullName + " to " + typeof(T).FullName);
            }
        }

        public T Convert<From, T>(From source) where T : new()
        {
            if (options.Value.ConvertFuncs.TryGetValue((typeof(From), typeof(T)), out var fun))
            {
                var fun1 = (Func<From, T>)fun;
                return fun1(source);
            }
            else
            {
                throw new Exception("Can not find convert mapper from " + typeof(From).FullName + " to " + typeof(T).FullName);
            }
        }

        public void Copy<From, T>(From source, T target, Action<object> onAddItem, Action<object> onRemoveItem)
            where T : class
        {
            if (options.Value.CopyFuncs.TryGetValue((typeof(From), typeof(T)), out var fun))
            {
                var fun1 = (Action<From, T, Action<object>, Action<object>>)fun;
                fun1(source, target, onAddItem, onRemoveItem);
            }
            else
            {
                throw new Exception("Can not find copy mapper from " + typeof(From).FullName + " to " + typeof(T).FullName);
            }
        }
    }

    public class FlyTigerMapperOptions
    {
        public Dictionary<(Type, Type), Delegate> QueryableFuncs = new Dictionary<(Type, Type), Delegate>();
        public Dictionary<(Type, Type), Delegate> ConvertFuncs = new Dictionary<(Type, Type), Delegate>();
        public Dictionary<(Type, Type), Delegate> CopyFuncs = new Dictionary<(Type, Type), Delegate>();
    }
}

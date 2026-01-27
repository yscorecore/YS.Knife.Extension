using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class DictionaryExtensions
    {
        public static void Merge<TK, TV>(this IDictionary<TK, TV> source, params IDictionary<TK, TV>[] dics)
        {
            _ = source ?? throw new ArgumentNullException(nameof(source));
            foreach (var dic in dics ?? Array.Empty<Dictionary<TK, TV>>())
            {
                if (dic != null)
                {
                    foreach (var (k, v) in dic)
                    {
                        source[k] = v;
                    }
                }
            }

        }
    }
}

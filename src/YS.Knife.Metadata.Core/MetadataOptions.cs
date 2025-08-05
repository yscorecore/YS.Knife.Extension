using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace YS.Knife.Metadata
{

    [Options]
    public class MetadataOptions
    {
        public IDictionary<string, Type> Metas { get; } = new Dictionary<string, Type>();

        public void AddMeta(string name, Type type)
        {
            if (Metas.ContainsKey(name))
            {
                throw new ArgumentException($"Metadata with name '{name}' already exists.");
            }
            Metas[name] = type;
        }
    }



}

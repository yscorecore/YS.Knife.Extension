using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
//using Microsoft.Extensions.Options;

namespace YS.Knife.Metadata
{

    //[Options]
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
        public void AddMeta(Type type)
        {
            AddMeta(type.GetCustomAttribute<MetadataAttribute>()?.Name ?? type.FullName, type);
        }
        public void AddMeta(params Type[] types)
        {
            Array.ForEach(types, AddMeta);
        }
        public void AddOrReplaceMeta(string name, Type type)
        {
            Metas[name] = type;
        }
        public void AddOrReplaceMeta(Type type)
        {
            AddOrReplaceMeta(type.GetCustomAttribute<MetadataAttribute>()?.Name ?? type.FullName, type);
        }
        public void AddOrReplaceMeta(params Type[] types)
        {
            Array.ForEach(types, AddOrReplaceMeta);
        }
    }



}

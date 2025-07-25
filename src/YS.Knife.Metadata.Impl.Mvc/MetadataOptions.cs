using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YS.Knife.Metadata.Impl.Mvc
{
    public class MetadataOptions
    {
        public IDictionary<string, Type> Metas { get; } = new Dictionary<string, Type>();
    }
}

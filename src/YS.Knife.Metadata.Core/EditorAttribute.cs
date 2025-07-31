using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YS.Knife.Metadata
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false)]
    public class EditorSourceAttribute : Attribute
    {
        public EditorSourceAttribute(string source)
        {
            this.Source = source;
        }
        public string Source { get; }
    }
}

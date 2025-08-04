using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YS.Knife.Metadata
{
    public interface IMetadataFilter
    {
        int Priority { get; }
        Task Process(MetadataFilterContext context, CancellationToken cancellationToken);
    }
}

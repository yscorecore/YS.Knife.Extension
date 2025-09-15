using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YS.Knife.FileStorage
{
    public interface IFileStreamInterceptor
    {
        string Name { get; }

        Stream HandlerStream(Stream inputStream, IDictionary<string, string> userArgs, CancellationToken cancellationToken);
    }
    public interface ISystemArgProvider
    {
        string DefaultFormatter { get; }
        object GetValue();
    }
}

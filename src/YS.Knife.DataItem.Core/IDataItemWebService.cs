using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YS.Knife.DataItem
{
    public interface IDataItemWebService
    {
        Task<Dictionary<string, object>> LoadData(string[] di, CancellationToken cancellationToken);
        Task<List<DataItemDesc>> ListItems();
    }
}

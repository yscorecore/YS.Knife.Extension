using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace YS.Knife.EnumCode
{
    public interface ICodeLoader
    {
        int Priority { get; }
        Task<Dictionary<string, List<IEnumCodeService.CodeInfo>>> AllCodes();
    }


}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YS.Knife.Function
{
    public record struct FunctionAssignObjectInfo
    {
        public AssignType Type { get; set; }

        //可以加上时间限制，生效时间等
        //public DateTime? StartTime { get; set; }
        //public DateTime? EndTime { get; set; }
    }
}

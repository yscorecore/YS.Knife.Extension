using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YS.Knife.Entity
{
    public interface ISoftDeleteEntity
    {
        public bool IsDeleted { get; set; }
    }
}

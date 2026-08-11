using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace YS.Knife.Function
{
    public interface ILayerSettingValue<T>
    {
        Task<T> Get();
        Task<T> GetByRoles(IDictionary<string, string[]> roleMaps);
        Task<TProp> GetPropertyValueByRoles<TProp>(IDictionary<string, string[]> roleMaps, Expression<Func<T, TProp>> propertyFunc);
    }
}

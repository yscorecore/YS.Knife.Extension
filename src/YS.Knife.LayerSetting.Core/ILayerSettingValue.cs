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
        Task<TProp> GetValue<TProp>(Expression<Func<T, TProp>> propertyFunc);
        Task<Tuple<TP1, TP2>> GetValue<TP1, TP2>(Expression<Func<T, TP1>> property1, Expression<Func<T, TP2>> property2);
        Task<Tuple<TP1, TP2, TP3>> GetValue<TP1, TP2, TP3>(Expression<Func<T, TP1>> property1, Expression<Func<T, TP2>> property2, Expression<Func<T, TP3>> property3);
        Task<Tuple<TP1, TP2, TP3, TP4>> GetValue<TP1, TP2, TP3, TP4>(Expression<Func<T, TP1>> property1, Expression<Func<T, TP2>> property2, Expression<Func<T, TP3>> property3, Expression<Func<T, TP4>> property4);
        Task<T> GetByRoles(IDictionary<string, string[]> roleMaps);
        Task<TProp> GetPropertyValueByRoles<TProp>(IDictionary<string, string[]> roleMaps, Expression<Func<T, TProp>> propertyFunc);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YS.Knife.Service
{
    public interface ICudServiceApi<TCreateDto, TUpdateDto, TKey> :
           ICreateApi<TCreateDto, TKey>
         , IUpdateApi<TUpdateDto, TKey>
         , IDeleteApi<TKey>
         where TCreateDto : class
         where TUpdateDto : class
    {

    }
    public interface ICudServiceApi<T, TKey> : ICudServiceApi<T, T, TKey>
        where T : class
    {

    }
}

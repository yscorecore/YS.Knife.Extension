using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace YS.Knife.Mapper.Impl.FlyTiger
{
    internal class ServiceRegister : IServiceRegister
    {
        public void RegisterServices(IServiceCollection services, IRegisterContext context)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(p => Attribute.IsDefined(p, typeof(MapperAssemblyAttribute)));
            services.AddFlyTigerMapper(assemblies.ToArray());
        }
    }
}

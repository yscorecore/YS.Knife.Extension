using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace YS.Knife.DataItem.Impl.Default
{
    internal class ServiceRegister : YS.Knife.IServiceRegister
    {
        public void RegisterServices(IServiceCollection services, IRegisterContext context)
        {
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
        }
    }
}

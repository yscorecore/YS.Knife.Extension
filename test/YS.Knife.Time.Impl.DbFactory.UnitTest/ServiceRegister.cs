using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace YS.Knife.Time.Impl.EFDatabaseFacade.UnitTest
{
    internal class ServiceRegister : YS.Knife.IServiceRegister
    {
        public void RegisterServices(IServiceCollection services, IRegisterContext context)
        {
            var conn = context.Configuration.GetConnectionString("TestDb");
            services.AddDbContext<TestDbContext>((op) =>
            {
                op.UseNpgsql(conn)
                    .EnableSensitiveDataLogging(true)
                    .LogTo(Console.WriteLine);
            });
        }
    }
}

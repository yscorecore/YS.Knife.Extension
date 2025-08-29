using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using YS.Knife.Data;

namespace YS.Knife.EFCore
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public partial class EFDbConnectionFactoryAttribute : KnifeAttribute
    {
        public EFDbConnectionFactoryAttribute() : base(typeof(DbContext))
        {

        }

        public override void RegisterService(IServiceCollection services, IRegisterContext context, Type declareType)
        {
            var implType = typeof(EFDatabaseConnectionFactory<>).MakeGenericType(declareType);
            var genericType = typeof(IDbConnectionFactory<>).MakeGenericType(declareType);
            services.AddScoped(genericType, implType);
            services.AddScoped(typeof(IDbConnectionFactory), (sp) => sp.GetRequiredService(genericType));
        }
    }

}

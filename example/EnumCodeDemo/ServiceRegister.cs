using YS.Knife.EnumCode.Impl.Default;

namespace EnumCodeDemo
{
    public class ServiceRegister : YS.Knife.IServiceRegister
    {
        public void RegisterServices(IServiceCollection services, YS.Knife.IRegisterContext context)
        {
            services.Configure<AssemblyEnumCodeOptions>(p => p.Assemblies = new[] { "EnumCodeDemo" });
        }
    }
}

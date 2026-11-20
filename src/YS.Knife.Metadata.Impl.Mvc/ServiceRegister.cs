
using System.Reflection;

namespace YS.Knife.Metadata.Impl.Mvc
{
    public class ServiceRegister : YS.Knife.IServiceRegister
    {
        public void RegisterServices(IServiceCollection services, IRegisterContext context)
        {
            // MetadataService 依赖 ASP.NET Core MVC 的 IModelMetadataProvider，
            // 且元数据语义（Description/Required/Order 等）来自 DataAnnotations，
            // 因此需注册 MVC 核心服务并启用 DataAnnotations 元数据提供程序。
            services.AddMvcCore().AddDataAnnotations();

            var allTypes = AppDomain.CurrentDomain.FindInstanceTypesByAttribute<MetadataAttribute>()
                  .ToArray();
            services.Configure<MetadataOptions>(t =>
            {
                t.AddOrReplaceMeta(allTypes);
            });
        }
    }
}

using Microsoft.Extensions.DependencyInjection;

namespace YS.Knife.Auth
{
    internal class ServiceRegister : IServiceRegister
    {
        public void RegisterServices(IServiceCollection services, IRegisterContext context)
        {
            // 手机号绑定等操作依赖 HttpContext 解析当前登录用户
            services.AddHttpContextAccessor();
            // 短信登录验证码存放于分布式缓存；宿主未配置时回退为进程内缓存
            services.AddDistributedMemoryCache();
        }
    }
}

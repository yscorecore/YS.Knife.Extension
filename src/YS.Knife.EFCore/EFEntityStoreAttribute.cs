using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using YS.Knife.Entity;

namespace YS.Knife.EFCore
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class EFEntityStoreAttribute : KnifeAttribute
    {
        public Type EntityType { get; private set; }
        public EFEntityStoreAttribute(Type type) : base(typeof(DbContext))
        {
            this.EntityType = type;
        }

        public override void RegisterService(IServiceCollection services, IRegisterContext context, Type declareType)
        {
            services.AddScoped(typeof(IEntityStore<>).MakeGenericType(this.EntityType), (sp) =>
            {
                var entityStoreType = typeof(EFEntityStore<,>).MakeGenericType(this.EntityType, declareType);
                return ActivatorUtilities.CreateInstance(sp, entityStoreType);
            });
        }
    }
}

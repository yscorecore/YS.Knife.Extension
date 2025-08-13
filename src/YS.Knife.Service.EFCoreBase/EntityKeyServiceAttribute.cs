using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace YS.Knife
{
    public class EntityKeyServiceAttribute : YS.Knife.ServiceAttribute
    {
        public EntityKeyServiceAttribute() : base()
        {
        }
        public EntityKeyServiceAttribute(Type injectType, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
            : base(injectType, serviceLifetime)
        {
        }
        public override void RegisterService(IServiceCollection services, IRegisterContext context, Type declareType)
        {
            if (declareType.IsGenericType && declareType.GetGenericArguments().Count() == 1)
            {
                var type = GetConfigEntityKeyType(context);
                var genericType = declareType.MakeGenericType(type);
                base.RegisterService(services, context, genericType);
            }
            else
            {
                throw new Exception($"EntityKeyServiceAttribute can only be used with generic types that have one type argument, but got '{declareType.FullName}'.");
            }
        }

        private static Type GetConfigEntityKeyType(IRegisterContext context)
        {
            var entityType = context.Configuration.GetValue<string>("ENTITYKEYTYPE");
            return entityType?.ToLowerInvariant() switch
            {
                null => typeof(Guid),
                "" => typeof(int),
                "int" => typeof(int),
                "long" => typeof(long),
                "string" => typeof(string),
                "guid" => typeof(Guid),
                "byte[]" => typeof(byte[]),
                _ => throw new NotSupportedException($"Entity key type '{entityType}' is not supported.")
            };
        }

    }
}

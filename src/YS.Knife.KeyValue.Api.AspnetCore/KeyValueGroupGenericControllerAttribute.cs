using System.Reflection;
using YS.Knife.AspnetCore.Mvc;

namespace YS.Knife.KeyValue.Api.AspnetCore
{
    public class KeyValueGroupGenericControllerAttribute : GenericControllerAttribute
    {
        public KeyValueGroupGenericControllerAttribute(Type genericControllerType) : base(genericControllerType)
        {
        }

        protected override IEnumerable<Type[]> GetAllGenericTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly =>
                {
                    return !assembly.FullName.StartsWith("System", StringComparison.OrdinalIgnoreCase)
                            && !assembly.FullName.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase);
                })
                  .SelectMany(assembly => assembly.GetTypes())
                  .Where(type => type.IsSubclassOf(typeof(KeyValueGroup)) && !type.IsAbstract && type.GetCustomAttribute<ServiceAttribute>() != null)
                  .Select(type => new[] { type });
        }

        protected override string ResolveControllerName(Type[] genericTypes)
        {
            return genericTypes[0].Name;
        }
    }
}

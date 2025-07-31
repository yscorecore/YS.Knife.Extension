using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace YS.Knife.AspnetCore.Mvc
{
    public class GenericControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            var attributes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(p => p.GetTypes())
                .Where(p => !p.IsAbstract && p.IsGenericType && typeof(ControllerBase).IsAssignableFrom(p) &&
                            p.IsDefined(typeof(GenericControllerAttribute), true))
                //.Where(p => !RegisterContext.HasFiltered(p))
                .Select(p => p.GetCustomAttribute<GenericControllerAttribute>()!);
            foreach (var genericAttribute in attributes)
            {
                genericAttribute.ApplyControllerFeature(parts, feature);
            }
        }
    }
}

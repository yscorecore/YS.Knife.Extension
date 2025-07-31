using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using YS.Knife.AspnetCore.Mvc;
namespace YS.Knife.DataSource.Api.AspnetCore
{
    public class DataSourceGenericControllerAttribute : GenericControllerAttribute, IControllerModelConvention
    {
        public DataSourceGenericControllerAttribute(Type genericControllerType) : base(genericControllerType)
        {
        }

        public override void Apply(ControllerModel controller)
        {
            base.Apply(controller);
            if (controller.ControllerType.IsGenericType == false)
            {
                return;
            }
            var routes = AssemblyDataSourceEntryFinder.Instance.All.Values.Where(p => p.EntityType == controller.ControllerType.GenericTypeArguments[0])
                     .Select(p => p.Name).ToList();
            var template = controller.Attributes.OfType<RouteAttribute>().Select(p => p.Template).FirstOrDefault();
            controller.Selectors.Clear();
            foreach (var route in routes)
            {
                controller.Selectors.Add(new SelectorModel
                {
                    AttributeRouteModel = new AttributeRouteModel(new RouteAttribute($"{template}/{route}")),
                });
                controller.RouteValues["datasourceName"] = route;
            }
        }


        protected override string ResolveControllerName(Type[] genericTypes)
        {
            var originalName = this.GenericControllerType.Name;
            var index = originalName.IndexOf('`');
            var normalName = originalName[..index];
            if (normalName.EndsWith("Controller", StringComparison.OrdinalIgnoreCase))
            {
                normalName = normalName[..^10];
            }
            return normalName;
        }

        protected override IEnumerable<Type[]> GetAllGenericTypes()
        {
            return AssemblyDataSourceEntryFinder.Instance.All.Values.Select(p => p.EntityType).Distinct()
                .Select(p => new[] { p });
        }
    }

}

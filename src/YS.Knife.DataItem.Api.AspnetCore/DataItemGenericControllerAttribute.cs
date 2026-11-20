using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using YS.Knife.AspnetCore.Mvc;
using YS.Knife.DataItem.Api.AspnetCore.Internal;
namespace YS.Knife.DataItem.Api.AspnetCore
{
    public partial class DataItemGenericControllerAttribute : GenericControllerAttribute, IControllerModelConvention
    {
        private const string MigicTypeNameSplitter = "_o_";
        private const int MaxArgCount = 8;

        public DataItemGenericControllerAttribute(Type genericControllerType) : base(genericControllerType)
        {
        }
        private (string dataItemName, IList<string> argNames) ParseName(Type type)
        {
            var items = type.Name.Split(MigicTypeNameSplitter, StringSplitOptions.RemoveEmptyEntries);
            return (items[0], new ArraySegment<string>(items, 1, items.Length - 1));
        }
        public override void Apply(ControllerModel controller)
        {
            base.Apply(controller);
            if (controller.ControllerType.IsGenericType == false)
            {
                return;
            }
            var genericTypeArgs = controller.ControllerType.GetGenericArguments();
            var (dataSourceName, argNames) = ParseName(controller.ControllerType.GenericTypeArguments[MaxArgCount + 1]);
            var template = controller.Attributes.OfType<RouteAttribute>().Select(p => p.Template).FirstOrDefault();
            controller.Selectors.Clear();
            var selector = new SelectorModel
            {
                AttributeRouteModel = new AttributeRouteModel(new RouteAttribute($"{template}/{dataSourceName}")),
            };
            selector.EndpointMetadata.Add(new DataItemNameAttribute(dataSourceName));
            controller.Selectors.Add(selector);


            foreach (var action in controller.Actions)
            {
                var allParemeters = action.Parameters.ToList();
                for (var i = 0; i < allParemeters.Count; i++)
                {
                    var p = allParemeters[i];
                    if (p.ParameterType == typeof(NullObject))
                    {
                        action.Parameters.Remove(p);
                    }
                    else if (p.BindingInfo?.BindingSource == BindingSource.Query)
                    {
                        p.BindingInfo.BinderModelName = argNames[i];
                    }
                }
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
            return AssemblyDataItemEntryFinder.Instance.All.Values.Select(DataItemEntryToGenericTypes);

            Type[] DataItemEntryToGenericTypes(DataItemEntry entry)
            {
                if (entry.Parameters.Length > MaxArgCount)
                {
                    throw new InvalidOperationException($"The count of the dataitem's parameter should less or equal {MaxArgCount}.");
                }
                var res = new Type[MaxArgCount + 2];
                res[0] = entry.ReturnType;
                var names = new List<string> { entry.Name };

                for (int i = 0; i < MaxArgCount; i++)
                {
                    if (i >= entry.Parameters.Length)
                    {
                        res[i + 1] = typeof(NullObject);
                    }
                    else
                    {
                        res[i + 1] = entry.Parameters[i].ParameterType;
                        names.Add(entry.Parameters[i].Name);
                    }
                }
                res[MaxArgCount + 1] = DynamicType.Instance.Create(string.Join(MigicTypeNameSplitter, names));
                return res;
            }
        }



    }

}

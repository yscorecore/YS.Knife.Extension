using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Microsoft.EntityFrameworkCore
{
    public static class ModelBuilderExtensions
    {
        public static void ApplyKnifeExtensions(this DbContext dbContext, ModelBuilder modelBuilder)
        {
            var provider = dbContext.Database.ProviderName;
            var model = modelBuilder.Model;
            foreach (var modelAttribute in dbContext.GetType().GetCustomAttributes(true).OfType<IModelAttribute>().FilterAndSort(provider))
            {
                modelAttribute.Apply(modelBuilder);
            }

            foreach (var method in dbContext.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Select(m => new
                {
                    MethodInfo = m,
                    Attr = m.GetCustomAttributes(true).OfType<IModelMethodAttribute>().ToList()
                })
                .Where(m => m.Attr.Any())
               )
            {
                foreach (var modelMethodAttribute in method.Attr.FilterAndSort(provider))
                {
                    modelMethodAttribute.Apply(modelBuilder, method.MethodInfo);
                }
            }

            foreach (var entityType in model.GetEntityTypes())
            {
                var clrType = entityType.ClrType;
                var modelAttributes = clrType.GetCustomAttributes(true).OfType<IModelTypeAttribute>().FilterAndSort(provider).ToList();
                if (clrType != null && modelAttributes.Any())
                {
                    var typeBuilder = modelBuilder.Entity(entityType.ClrType);
                    foreach (var typeAttribute in modelAttributes)
                    {
                        typeAttribute.Apply(typeBuilder);
                    }
                }
                var allProperties = clrType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Select(p => new
                    {
                        Prop = p,
                        Attrs = p.GetCustomAttributes(true).OfType<IModelPropertyAttribute>().ToList()
                    })
                    .Where(p => p.Attrs.Any()).ToList();
                if (clrType != null && allProperties.Any())
                {
                    var typeBuilder = modelBuilder.Entity(entityType.ClrType);
                    foreach (var p in allProperties)
                    {
                        var propBuilder = typeBuilder.Property(p.Prop.Name);
                        foreach (var propertyAttribute in p.Attrs.FilterAndSort(provider))
                        {
                            propertyAttribute.Apply(propBuilder);
                        }
                    }
                }

            }
        }
        private static IEnumerable<T> FilterAndSort<T>(this IEnumerable<T> source, string provider)
        {
            foreach (var item in source)
            {
                if (item is not IProviderAttribute)
                {
                    yield return item;
                }
            }
            foreach (var item in source)
            {
                if (item is IProviderAttribute providerAttribute && string.IsNullOrEmpty(providerAttribute.Provider))
                {
                    yield return item;
                }
            }
            foreach (var item in source)
            {
                if (item is IProviderAttribute providerAttribute && string.Equals(providerAttribute.Provider, provider, StringComparison.OrdinalIgnoreCase))
                {
                    yield return item;
                }
            }
        }

    }
}

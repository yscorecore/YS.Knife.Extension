using System;
using System.Reflection;

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
                modelAttribute.Apply(model);
            }

            foreach (var entityType in model.GetEntityTypes())
            {
                var clrType = entityType.ClrType;
                if (clrType != null)
                {
                    foreach (var typeAttribute in clrType.GetCustomAttributes(true).OfType<IModelTypeAttribute>().FilterAndSort(provider))
                    {
                        typeAttribute.Apply(entityType);
                    }
                }
                foreach (var property in entityType.GetProperties())
                {
                    var memberInfo = property.PropertyInfo ?? (MemberInfo)property.FieldInfo;
                    if (memberInfo == null) continue;
                    foreach (var propertyAttribute in memberInfo.GetCustomAttributes(true).OfType<IModelPropertyAttribute>().FilterAndSort(provider))
                    {
                        propertyAttribute.Apply(property);
                    }
                }
            }
        }
        private static IEnumerable<T> FilterAndSort<T>(this IEnumerable<T> source, string provider)
        {
            foreach (var item in source)
            {
                if (item is not ProviderAttribute)
                {
                    yield return item;
                }
            }
            foreach (var item in source)
            {
                if (item is ProviderAttribute providerAttribute && string.IsNullOrEmpty(providerAttribute.Provider))
                {
                    yield return item;
                }
            }
            foreach (var item in source)
            {
                if (item is ProviderAttribute providerAttribute && string.Equals(providerAttribute.Provider, provider, StringComparison.OrdinalIgnoreCase))
                {
                    yield return item;
                }
            }
        }
        private static bool ShouldUse(object attribute, string provider)
        {
            if (attribute is ProviderAttribute providerAttribute)
            {
                if (string.IsNullOrEmpty(providerAttribute.Provider))
                {
                    return true;
                }
                return string.Equals(providerAttribute.Provider, provider, StringComparison.OrdinalIgnoreCase);
            }
            return true;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace YS.Knife.Metadata.Impl.Mvc
{
    public static class ModelMetadataProviderExtensions
    {
        public static MetadataInfo GetMetadataInfoFromType(this IModelMetadataProvider metadataProvider, Type type)
        {
            var model = metadataProvider.GetMetadataForType(type);

            return new MetadataInfo
            {
                DisplayName = model.DisplayName ?? type.Name,
                Description = model.Description,
                Columns = model.Properties.Select(PropertyToMetadataClolumnInfo).ToList()
            };
        }
        private static MetadataClolumnInfo PropertyToMetadataClolumnInfo(ModelMetadata p)
        {
            var (isArray, typeCode) = GetTypeCode(p.ModelType);
            return new MetadataClolumnInfo
            {
                DisplayName = GetDisplayName(p),
                PropertyPath = p.PropertyName,
                ShowForDisplay = p.ShowForDisplay,
                DisplayFormat = p.DisplayFormatString,
                // DataTypeName= p.DataTypeName
                Description = p.Description,
                IsArray = isArray,
                DataTypeName = typeCode
            };
        }
        private static string GetDisplayName(ModelMetadata metadata)
        {
            return metadata.DisplayName ?? metadata.PropertyName;
        }
        private static (bool, string) GetTypeCode(Type type)
        {
            if (type.IsArray)
            {
                return (true, GetTypeCode(type!.GetElementType()));
            }
            if (Type.GetTypeCode(type) != TypeCode.Object)
            {
                return (false, GetTypeCode(type));
            }
            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                var itemType = type.GetInterfaces()
                    .Where(p => p.IsGenericType && p.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    .Select(p => p.GetGenericArguments().First()).DefaultIfEmpty(typeof(object)).FirstOrDefault();
                return (true, GetTypeCode(itemType));
            }
            return (false, GetTypeCode(type));
            static string GetTypeCode(Type type)
            {
                return Type.GetTypeCode(type).ToString().ToLowerInvariant();
            }

        }
    }
}

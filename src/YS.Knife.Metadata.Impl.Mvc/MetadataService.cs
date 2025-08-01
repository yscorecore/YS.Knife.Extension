
using System.Collections;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.Options;

namespace YS.Knife.Metadata.Impl.Mvc
{
    [Service]
    [FlyTiger.AutoConstructor]
    public partial class MetadataService : IMetadataService
    {
        private readonly IModelMetadataProvider modelMetadataProvider;
        private readonly IOptions<MetadataOptions> metadataOptions;
        private readonly IOptions<JsonOptions> jsonOptions;


        public Task<MetadataInfo> GetMetadataInfo(string name, CancellationToken cancellationToken = default)
        {
            if (metadataOptions.Value.Metas.TryGetValue(name, out var type))
            {
                return Task.FromResult(GetMetadataInfoFromType(type));
            }
            throw new Exception($"Can not find data meta by name '{name}'.");

        }

        public Task<List<string>> ListAllNames(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(metadataOptions.Value.Metas.Keys.ToList());
        }


        private MetadataInfo GetMetadataInfoFromType(Type type)
        {
            var model = modelMetadataProvider.GetMetadataForType(type);

            return new MetadataInfo
            {
                DisplayName = model.DisplayName ?? type.Name,
                Description = model.Description,
                Columns = model.Properties.Select(p => PropertyToMetadataClolumnInfo(p as DefaultModelMetadata)).ToList()
            };
        }
        private MetadataClolumnInfo PropertyToMetadataClolumnInfo(DefaultModelMetadata p)
        {
            var (isArray, typeCode) = GetTypeCode(p.ModelType);

            return new MetadataClolumnInfo
            {
                PropertyPath = GetPropertyPath(p),
                DisplayName = GetDisplayName(p),
                ShowForDisplay = p.ShowForDisplay,
                DisplayFormat = p.DisplayFormatString,
                Description = GetDescription(p),
                IsArray = isArray,
                DataTypeName = typeCode,
                EditorSource = p.Attributes.Attributes.OfType<EditorSourceAttribute>().Select(p => p.Source).FirstOrDefault()
            };
        }
        private string GetPropertyPath(DefaultModelMetadata metadata)
        {
            if (metadata.Attributes.Attributes.OfType<JsonPropertyNameAttribute>().FirstOrDefault() is { } jsonPropertyName)
            {
                return jsonPropertyName.Name;
            }
            var namePolicy = jsonOptions.Value.SerializerOptions.PropertyNamingPolicy;
            if (namePolicy is not null && namePolicy.ConvertName(metadata.PropertyName) is { } convertedName)
            {
                return convertedName;
            }
            return metadata.PropertyName;
        }
        private static string GetDisplayName(DefaultModelMetadata metadata)
        {
            return metadata.DisplayName ?? metadata.PropertyName;
        }
        private static string GetDescription(DefaultModelMetadata metadata)
        {
            return metadata.Description ?? (
                metadata.Attributes.Attributes.OfType<DescriptionAttribute>().Select(p => p.Description).FirstOrDefault()
                );
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

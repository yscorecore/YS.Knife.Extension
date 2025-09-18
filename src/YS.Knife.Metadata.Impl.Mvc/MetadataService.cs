
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
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
        private readonly IEnumerable<IMetadataInterceptor> metadataInterceptors;

        public async Task<MetadataInfo> GetMetadataInfo(string name, CancellationToken cancellationToken = default)
        {
            if (metadataOptions.Value.Metas.TryGetValue(name, out var type))
            {
                var (metadataInfo, interceptors) = GetMetadataInfoFromType(type);
                return await ExecuteInterceptors(name, metadataInfo, interceptors, cancellationToken);
            }
            throw new Exception($"Can not find metadata by name '{name}'.");
        }
        private async Task<MetadataInfo> ExecuteInterceptors(string name, MetadataInfo metadataInfo, string[] filters, CancellationToken cancellationToken)
        {
            if (metadataInterceptors.Any())
            {
                foreach (var interceptor in metadataInterceptors.Where(p => p.IsGlobal || filters.Contains(p.Name)).OrderBy(p => p.Priority))
                {
                    var context = new MetadataInterceptorContext
                    {
                        MetadataName = name,
                        MetadataInfo = metadataInfo
                    };
                    await interceptor.Process(context, cancellationToken);
                    metadataInfo = context.MetadataInfo;
                }
            }
            return metadataInfo;
        }
        public Task<List<string>> ListAllNames(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(metadataOptions.Value.Metas.Keys.ToList());
        }
        private (MetadataInfo Metadata, string[] Interceptors) GetMetadataInfoFromType(Type type)
        {
            var model = modelMetadataProvider.GetMetadataForType(type);
            return (new MetadataInfo
            {
                DisplayName = model.DisplayName ?? type.Name,
                Description = model.Description,
                Columns = GetMetadataColumns(model, new HashSet<Type>()).ToList()
            },
            type.GetCustomAttributes<MetadataInterceptorAttribute>().Select(p => p.InterceptorName).ToArray()
            );
        }

        private IEnumerable<MetadataClolumnInfo> GetMetadataColumns(ModelMetadata model, ISet<Type> handledTypes)
        {
            handledTypes.Add(model.ModelType);
            foreach (var c in model.Properties.Where(p => !handledTypes.Contains(p.ModelType))
                .OrderBy(p => p.Order)
                .Select(p => PropertyToMetadataClolumnInfo(p)))
            {
                if (c.IsArray == false && c.DataTypeName == "object")
                {
                    var newSet = handledTypes.ToHashSet();
                    foreach (var v in GetMetadataColumns(c.PropertyModel, newSet))
                    {
                        v.PropertyPath = $"{c.PropertyPath}.{v.PropertyPath}";
                        yield return v;
                    }
                }
                else
                {
                    yield return c;
                }
            }

        }


        private MetadataClolumnInfo2 PropertyToMetadataClolumnInfo(ModelMetadata c)
        {
            var p = c as DefaultModelMetadata;
            var (isArray, typeCode, type) = GetTypeCode(p.ModelType);
            return new MetadataClolumnInfo2
            {
                PropertyModel = p,
                PropertyPath = GetPropertyPath(p),
                DisplayName = GetDisplayName(p),
                ShowForDisplay = p.ShowForDisplay,
                DisplayFormat = p.DisplayFormatString,
                Description = GetDescription(p),
                IsArray = isArray,
                DataTypeName = typeCode,
                DisplayOrder = p.Order,
                DataSource = GetEditorSource(p, type),
                QueryFilter = GetQueryFilterInfo(p),
            };
        }
        private DataSourceInfo GetEditorSource(DefaultModelMetadata metadata, Type itemType)
        {
            var source = metadata.Attributes.PropertyAttributes.OfType<EditorSourceAttribute>().FirstOrDefault();
            if (source == null && itemType.IsEnum)
            {
                source = new EditorEnumSourceAttribute(itemType.Name);
            }
            return source == null ? null : new DataSourceInfo
            {
                Type = source.SourceType,
                Value = source.Value
            };
        }
        private QueryFilterInfo GetQueryFilterInfo(DefaultModelMetadata metadata)
        {
            var filter = metadata.Attributes.PropertyAttributes.OfType<QueryFilterAttribute>().FirstOrDefault();
            if (filter == null)
            {
                return null;
            }
            return new QueryFilterInfo
            {
                DefaultValue = filter.DefaultValue,
                Operator = filter.Operator,
            };
        }
        private string GetPropertyPath(DefaultModelMetadata metadata)
        {
            if (metadata.Attributes.PropertyAttributes.OfType<JsonPropertyNameAttribute>().FirstOrDefault() is { } jsonPropertyName)
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
            if (metadata.Attributes.PropertyAttributes.OfType<DisplayAttribute>().FirstOrDefault() is { } displayAttribute)
            {
                return displayAttribute.Name;
            }
            if (metadata.Attributes.PropertyAttributes.OfType<DisplayNameAttribute>().FirstOrDefault() is { } displayNameAttribute)
            {
                return displayNameAttribute.DisplayName;
            }
            return metadata.PropertyName;
        }
        private static string GetDescription(DefaultModelMetadata metadata)
        {
            return metadata.Description ?? (
                metadata.Attributes.PropertyAttributes.OfType<DescriptionAttribute>().Select(p => p.Description).FirstOrDefault()
                );
        }
        private static (bool, string, Type) GetTypeCode(Type type)
        {
            type = ExcludeNullableType(type);
            if (type.IsArray)
            {
                return (true, GetTypeCode(type!.GetElementType()), type!.GetElementType());
            }
            if (Type.GetTypeCode(type) != TypeCode.Object)
            {
                return (false, GetTypeCode(type), type);
            }
            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                var itemType = type.GetInterfaces()
                    .Where(p => p.IsGenericType && p.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    .Select(p => p.GetGenericArguments().First()).DefaultIfEmpty(typeof(object)).FirstOrDefault();
                return (true, GetTypeCode(itemType), itemType);
            }
            return (false, GetTypeCode(type), type);
            static string GetTypeCode(Type type)
            {

                return Type.GetTypeCode(type).ToString().ToLowerInvariant();
            }
            static Type ExcludeNullableType(Type type)
            {
                return Nullable.GetUnderlyingType(type) ?? type;
            }
        }
        internal record MetadataClolumnInfo2 : MetadataClolumnInfo
        {
            public DefaultModelMetadata PropertyModel { get; internal set; }
        }
    }
}

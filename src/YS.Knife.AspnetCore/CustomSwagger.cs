using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Routing;

namespace YS.Knife
{

    public partial class CustomSwagger
    {
        public static CustomSwagger Instance { get; } = new CustomSwagger();

        private readonly ConcurrentDictionary<string, List<string>> _schemaNameRepetition = new();

        // borrowed from https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/95cb4d370e08e54eb04cf14e7e6388ca974a686e/src/Swashbuckle.AspNetCore.SwaggerGen/SchemaGenerator/SchemaGeneratorOptions.cs#L44
        private string DefaultSchemaIdSelector(Type modelType)
        {
            if (!modelType.IsConstructedGenericType) return modelType.Name.Replace("[]", "Array");

            var prefix = modelType.GetGenericArguments()
                .Select(genericArg => DefaultSchemaIdSelector(genericArg))
                .Aggregate((previous, current) => previous + current);

            return prefix + modelType.Name.Split('`').First();
        }

        public string GetUniqueSchemaId(Type modelType)
        {
            string id = DefaultSchemaIdSelector(modelType);

            if (!_schemaNameRepetition.ContainsKey(id))
                _schemaNameRepetition[id] = new List<string>();

            var modelNameList = _schemaNameRepetition[id];
            var fullName = modelType.FullName ?? string.Empty;
            if (!string.IsNullOrEmpty(fullName) && !modelNameList.Contains(fullName))
                modelNameList.Add(fullName);

            int index = modelNameList.IndexOf(fullName);

            return $"{id}{(index >= 1 ? index.ToString() : string.Empty)}";
        }
        public string GetOperationId(ApiDescription apiDesc)
        {
            // 优先使用HttpMethod特性指定的Name
            var httpMethodAttribute = apiDesc.ActionDescriptor.EndpointMetadata
                .OfType<HttpMethodAttribute>()
                .FirstOrDefault();

            if (!string.IsNullOrEmpty(httpMethodAttribute?.Name))
            {
                return httpMethodAttribute.Name;
            }

            // 如果没有指定，则使用方法名
            if (apiDesc.ActionDescriptor is ControllerActionDescriptor actionDescriptor)
            {
                return actionDescriptor.ActionName;
            }

            // 默认生成规则
            return apiDesc.ActionDescriptor.RouteValues["action"];
        }
    }
}

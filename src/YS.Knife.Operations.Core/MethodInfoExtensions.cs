using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace YS.Knife.Operations
{
    public static class MethodInfoExtensions
    {
        private static System.Collections.Concurrent.ConcurrentDictionary<MethodInfo, Operation> operationCaches = new System.Collections.Concurrent.ConcurrentDictionary<MethodInfo, Operation>();
        public static Operation GetOperation(this MethodInfo methodInfo)
        {
            return operationCaches.GetOrAdd(methodInfo, p =>
            {
                var attr = methodInfo.GetCustomAttribute<OperationAttribute>();
                if (attr == null)
                {
                    return new Operation { Id = methodInfo.Name };
                }
                var genericArguments = GetGenericArguments(methodInfo);
                if (genericArguments.Length == 0)
                {
                    // 非泛型类型的方法：保持原样，兼容旧用法
                    return new Operation { Id = attr.Id, Description = attr.Description };
                }
                var idArgs = new object[genericArguments.Length];
                var descriptionArgs = new object[genericArguments.Length];
                for (var i = 0; i < genericArguments.Length; i++)
                {
                    idArgs[i] = FormatTypeName(genericArguments[i].Name, attr.NamingStyle);
                    descriptionArgs[i] = GetTypeDescription(genericArguments[i]);
                }
                return new Operation
                {
                    Id = FormatTemplate(attr.Id, idArgs),
                    Description = FormatTemplate(attr.Description, descriptionArgs),
                };
            });
        }

        private static Type[] GetGenericArguments(MethodInfo methodInfo)
        {
            var declaringType = methodInfo.DeclaringType;
            return declaringType != null && declaringType.IsGenericType
                ? declaringType.GetGenericArguments()
                : Type.EmptyTypes;
        }

        private static string FormatTemplate(string template, object[] args)
        {
            return template == null ? null : string.Format(CultureInfo.InvariantCulture, template, args);
        }

        private static string GetTypeDescription(Type type)
        {
            var descriptionAttribute = type.GetCustomAttribute<DescriptionAttribute>();
            return descriptionAttribute == null ? type.Name : descriptionAttribute.Description;
        }

        private static string FormatTypeName(string name, OperationNamingStyle namingStyle)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }
            switch (namingStyle)
            {
                case OperationNamingStyle.CamelCase:
                    return char.ToLowerInvariant(name[0]) + name.Substring(1);
                case OperationNamingStyle.PascalCase:
                    return char.ToUpperInvariant(name[0]) + name.Substring(1);
                case OperationNamingStyle.LowerCase:
                    return name.ToLowerInvariant();
                case OperationNamingStyle.UpperCase:
                    return name.ToUpperInvariant();
                case OperationNamingStyle.Original:
                default:
                    return name;
            }
        }
    }
}


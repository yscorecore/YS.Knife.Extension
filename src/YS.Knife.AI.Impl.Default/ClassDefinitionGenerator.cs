using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace YS.Knife.AI.Impl.Default
{
    [Service(typeof(ClassDefinitionGenerator))]
    public class ClassDefinitionGenerator
    {
        public string GetClassDefinition(Type type)
        {
            // 1. 收集所有需要输出定义的类型（包括传入类型及其依赖）
            var allTypes = new HashSet<Type>();
            CollectDependentTypes(type, allTypes);

            // 2. 构建最终代码
            var sb = new StringBuilder();

            // 添加必要的 using 语句
            sb.AppendLine("using System;");
            sb.AppendLine("using System.ComponentModel;");
            sb.AppendLine("using System.ComponentModel.DataAnnotations;");
            sb.AppendLine();

            // 按命名空间分组，每个命名空间独立输出
            var groups = allTypes.GroupBy(t => t.Namespace ?? "global");
            foreach (var group in groups)
            {
                string ns = group.Key;
                if (!string.IsNullOrEmpty(ns) && ns != "global")
                {
                    sb.AppendLine($"namespace {ns}");
                    sb.AppendLine("{");
                }

                foreach (var t in group.OrderBy(t => t.Name))
                {
                    sb.AppendLine(GenerateClassDefinition(t));
                }

                if (!string.IsNullOrEmpty(ns) && ns != "global")
                {
                    sb.AppendLine("}");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// 递归收集所有依赖的类型（避免循环引用）
        /// </summary>
        private void CollectDependentTypes(Type type, HashSet<Type> collected)
        {
            if (type == null || collected.Contains(type))
                return;

            // 只收集需要生成定义的自定义类型
            if (!ShouldGenerateTypeDefinition(type))
                return;

            collected.Add(type);

            // 遍历所有公共实例属性，提取属性类型中的依赖
            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var propType = prop.PropertyType;
                foreach (var dependentType in ExtractRelevantTypes(propType))
                {
                    CollectDependentTypes(dependentType, collected);
                }
            }
        }

        /// <summary>
        /// 判断一个类型是否需要生成独立的类定义
        /// </summary>
        private bool ShouldGenerateTypeDefinition(Type type)
        {
            // 排除基元类型、字符串、decimal、DateTime、枚举等
            if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal) || type == typeof(DateTime))
                return false;
            if (type.IsEnum)
                return false;

            // 排除系统程序集中的类型（例如 System.Object, System.Collections.Generic.List<> 等）
            var systemAssemblyNames = new[] { "mscorlib", "System.Private.CoreLib", "System", "System.Runtime" };
            string? assemblyName = type.Assembly.GetName().Name;
            if (assemblyName ==null || systemAssemblyNames.Contains(assemblyName))
                return false;

            // 只处理类或结构体（接口也可以，但实际使用较少，按需调整）
            return type.IsClass || type.IsValueType;
        }

        /// <summary>
        /// 从一个类型中提取所有“需要关注”的自定义类型（例如泛型参数、数组元素、Nullable 基础类型）
        /// </summary>
        private IEnumerable<Type> ExtractRelevantTypes(Type type)
        {
            // 处理数组
            if (type.IsArray)
            {
                yield return type.GetElementType()!;
                yield break;
            }

            // 处理泛型
            if (type.IsGenericType)
            {
                foreach (var arg in type.GetGenericArguments())
                    yield return arg;
                yield break;
            }

            // 处理 Nullable<T>
            if (Nullable.GetUnderlyingType(type) is Type nullableUnderlying)
            {
                yield return nullableUnderlying;
                yield break;
            }

            // 普通类型
            yield return type;
        }

        /// <summary>
        /// 生成单个类型的类定义字符串（不包含命名空间和 using）
        /// </summary>
        private string GenerateClassDefinition(Type type)
        {
            var sb = new StringBuilder();

            // 类修饰符和关键字
            string accessibility = type.IsPublic ? "public" : "internal";
            string kind = type.IsClass ? "class" : (type.IsValueType && !type.IsEnum ? "struct" : "class");
            sb.AppendLine($"    {accessibility} {kind} {type.Name}");
            sb.AppendLine("    {");

            // 处理属性
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in properties)
            {
                // DescriptionAttribute
                var descAttr = prop.GetCustomAttribute<DescriptionAttribute>();
                if (descAttr != null)
                {
                    sb.AppendLine($"        [Description(\"{EscapeString(descAttr.Description)}\")]");
                }
                else
                {
                    var displayAttr = prop.GetCustomAttribute<DisplayAttribute>();
                    if (displayAttr != null)
                        {
                            var displayParams = new List<string>();
                            if (!string.IsNullOrEmpty(displayAttr.Name))
                                displayParams.Add($"Name = \"{EscapeString(displayAttr.Name)}\"");
                            if (!string.IsNullOrEmpty(displayAttr.Description))
                                displayParams.Add($"Description = \"{EscapeString(displayAttr.Description)}\"");
                            var order = displayAttr.GetOrder();
                            if (order != null)
                                displayParams.Add($"Order = {order}");
                            if (!string.IsNullOrEmpty(displayAttr.GroupName))
                                displayParams.Add($"GroupName = \"{EscapeString(displayAttr.GroupName)}\"");
                            string displayCode = displayParams.Count > 0
                                ? $"[Display({string.Join(", ", displayParams)})]"
                                : "[Display]";
                            sb.AppendLine($"        {displayCode}");
                        }
                }

                    // DisplayAttribute
              

                // 属性本身
                string propertyTypeName = GetFriendlyTypeName(prop.PropertyType);
                sb.AppendLine($"        public {propertyTypeName} {prop.Name} {{ get; set; }}");
                sb.AppendLine();
            }

            sb.AppendLine("    }");
            return sb.ToString();
        }

        private string EscapeString(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            return input.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        private readonly Dictionary<Type, string> _typeKeywords = new Dictionary<Type, string>
        {
            { typeof(string), "string" },
            { typeof(int), "int" },
            { typeof(uint), "uint" },
            { typeof(long), "long" },
            { typeof(ulong), "ulong" },
            { typeof(short), "short" },
            { typeof(ushort), "ushort" },
            { typeof(byte), "byte" },
            { typeof(sbyte), "sbyte" },
            { typeof(double), "double" },
            { typeof(float), "float" },
            { typeof(decimal), "decimal" },
            { typeof(bool), "bool" },
            { typeof(char), "char" },
            { typeof(object), "object" },
            { typeof(void), "void" }
        };

        private string GetFriendlyTypeName(Type type)
        {
            // 处理 Nullable<T>，写成 T? 的形式
            if (Nullable.GetUnderlyingType(type) is Type nullableUnderlying)
            {
                return $"{GetFriendlyTypeName(nullableUnderlying)}?";
            }

            // 处理关键字类型
            if (_typeKeywords.TryGetValue(type, out string keyword))
            {
                return keyword;
            }

            // 处理泛型类型
            if (type.IsGenericType)
            {
                var genericArgs = type.GetGenericArguments();
                var baseName = type.Name.Substring(0, type.Name.IndexOf('`'));
                var args = string.Join(", ", genericArgs.Select(GetFriendlyTypeName));
                return $"{baseName}<{args}>";
            }

            // 普通类型
            return type.Name;
        }
    }
}

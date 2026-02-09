using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using FlyTiger;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace YS.Knife.Generators.ExposeApi
{
    [Generator]
    public class ControllerGenerator : IIncrementalGenerator
    {
        const string NameSpaceName = "YS.Knife";
        const string AttributeName = "ExposeApiAttribute";
        internal static string AttributeFullName = $"{NameSpaceName}.{AttributeName}";

        private static readonly Dictionary<string, string[]> HttpMethodRules = new Dictionary<string, string[]>
        {
            {
                "HttpGet", new string[] { "get", "query", "find", "fetch" }
            },
            {
                "HttpPost", new string[] { "create", "add", "post", "upload", "save"}
            },
            {
                "HttpPut", new string[] { "update", "modify", "edit" }
            },
            {
                "HttpDelete", new string[] { "delete", "remove" }
            },
            {
                "HttpPatch", new string[] { "patch" }
            }
        };
        private static readonly HashSet<string> RouteFieldNames = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
        {
            "id", "name", "key"
        };

        const string AttributeCode = @"using System;
namespace YS.Knife
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    sealed class ExposeApiAttribute : Attribute
    {
        /// <summary>
        /// 要注入的服务类型
        /// </summary>
        public Type ServiceType { get; }
        
        /// <summary>
        /// Controller的路由前缀
        /// </summary>
        public string RoutePrefix { get; set; } = ""api/[controller]"";

        /// <summary>
        /// 允许匿名访问
        /// </summary>
        public bool AllowAnonymous { get; set; } = false;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name=""serviceType"">要注入的服务类型</param>
        public ExposeApiAttribute(Type serviceType)
        {
            ServiceType = serviceType;
        }
    }
}
";


        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Register the attribute source code
            context.RegisterPostInitializationOutput(i =>
            {
                i.AddSource($"{AttributeFullName}.g.cs", AttributeCode);
            });
            // 注册语法接收器
            var classDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (node, _) => node is ClassDeclarationSyntax { AttributeLists.Count: > 0 },
                    transform: static (context, _) => (ClassDeclarationSyntax)context.Node)
                .Where(static node => node != null);

            // 结合编译信息和类声明
            var compilationAndClasses = context.CompilationProvider.Combine(
                classDeclarations.Collect());

            // 注册源输出，只有在依赖类型变化时才会重新生成
            context.RegisterSourceOutput(compilationAndClasses, static (spc, source) =>
                Execute(source.Left, source.Right, spc));
        }

        private static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> classDeclarations, SourceProductionContext context)
        {
            var dynamicApiAttributeType = compilation.GetTypeByMetadataName(AttributeFullName);

            var codeWriter = new CodeWriter(compilation, context);
            // 查找所有标记了DynamicApiAttribute的类
            foreach (var classDecl in classDeclarations)
            {
                var semanticModel = compilation.GetSemanticModel(classDecl.SyntaxTree);
                var symbol = semanticModel.GetDeclaredSymbol(classDecl);
                if (symbol == null)
                    continue;
                var attrs = symbol.GetAttributes().Where(p => p.AttributeClass != null && p.AttributeClass.Equals(dynamicApiAttributeType, SymbolEqualityComparer.Default))
                        .ToImmutableList();
                foreach (var attr in attrs)
                {
                    GeneratorController(classDecl, attr, codeWriter);
                }
            }

        }
        private static void GeneratorController(ClassDeclarationSyntax classDeclarationSyntax, AttributeData attributeData, CodeWriter writer)
        {
            // 获取服务类型
            var serviceTypeSymbol = attributeData.ConstructorArguments[0].Value as INamedTypeSymbol;

            if (serviceTypeSymbol is null)
            {
                return;
            }

            var serviceType = serviceTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            // 确定控制器名称：服务类型名称 + Controller
            var controllerName = GetControllerName(serviceTypeSymbol, writer);

            // 确定服务名称：服务类型名称的驼峰式命名（首字母小写）
            var serviceName = GetServiceFieldName(serviceTypeSymbol);

            // 确定命名空间：与原始类相同
            var namespaceName = classDeclarationSyntax.Parent is NamespaceDeclarationSyntax namespaceDecl
                ? namespaceDecl.Name.ToString()
                : "";

            // 获取路由前缀
            var router = GetRouter() ?? "api/[controller]"; // 默认值

            var allowAnonymous = GetAllowAnonymous();

            CsharpCodeBuilder codeBuilder = new CsharpCodeBuilder("CS1591", "CS1573");

            codeBuilder.AppendCodeLines($@"using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace {namespaceName}");
            codeBuilder.BeginSegment();
            var comment = GetClassComment(serviceTypeSymbol);
            if (!string.IsNullOrEmpty(comment))
            {
                codeBuilder.AppendCodeLines(comment);
            }
            codeBuilder.AppendCodeLines($@"[ApiController]
[Route(""{router}"")]
");
            if (allowAnonymous)
            {
                codeBuilder.AppendCodeLines("[AllowAnonymous]");
            }
            codeBuilder.AppendCodeLines($"public class {controllerName} : ControllerBase");
            codeBuilder.BeginSegment();
            codeBuilder.AppendCodeLines($@"private readonly {serviceType} {serviceName};");
            //构造函数
            codeBuilder.AppendCodeLines($@"public {controllerName}({serviceType} {serviceName})
{{
    this.{serviceName} = {serviceName};
}}");

            foreach (var method in serviceTypeSymbol
                .GetAllMembers()
                .OfType<IMethodSymbol>()
                .Where(m => m.MethodKind == MethodKind.Ordinary && m.IsGenericMethod == false && m.DeclaredAccessibility == Accessibility.Public && m.ContainingType.SpecialType != SpecialType.System_Object && !m.IsStatic))
            {
                codeBuilder.AppendLine();
                codeBuilder.AppendCodeLines(GeneratorMethodCode(serviceTypeSymbol, serviceName, method));
            }

            codeBuilder.EndAllSegments();

            var code = codeBuilder.ToString();

            writer.WriteCodeFile(new CodeFile
            {
                BasicName = $"{namespaceName}.{controllerName}",
                Content = code
            });

            string GetRouter()
            {
                var routePrefixArg = attributeData.NamedArguments.FirstOrDefault(arg => arg.Key == "RoutePrefix");
                if (routePrefixArg.Value.Value != null)
                {
                    return (string)routePrefixArg.Value.Value;
                }
                return "api/[controller]";
            }
            bool GetAllowAnonymous()
            {
                var routePrefixArg = attributeData.NamedArguments.FirstOrDefault(arg => arg.Key == "AllowAnonymous");
                if (routePrefixArg.Value.Value != null)
                {
                    return (bool)routePrefixArg.Value.Value;
                }
                return false;
            }
            string GetServiceFieldName(INamedTypeSymbol serviceType)
            {
                return serviceTypeSymbol.Name.ToCamelCase();
            }
            string GetControllerName(INamedTypeSymbol serviceType, CodeWriter codeWriter)
            {
                var allNames = GetTypeNames(serviceType);
                var name = allNames[0];
                if (serviceType.TypeKind == TypeKind.Interface && allNames[0].StartsWith("I"))
                {
                    allNames[0] = allNames[0].Substring(1);
                }
                if (allNames[0].EndsWith("Service"))
                {
                    allNames[0] = allNames[0].Substring(0, allNames[0].Length - "Service".Length);
                }
                var fullName = string.Join("", allNames);
                var uniqueFullName = codeWriter.GetUniqueCodeName("Controller", fullName);
                return $"{uniqueFullName}Controller";
            }
            List<string> GetTypeNames(INamedTypeSymbol serviceType)
            {
                var res = new List<string>();
                if (serviceType.IsGenericType)
                {
                    var name = serviceTypeSymbol.Name;
                    var index = name.IndexOf('`');
                    res.Add(name.Substring(0, index - 1));
                    foreach (var type in serviceType.TypeArguments.OfType<INamedTypeSymbol>())
                    {
                        res.AddRange(GetTypeNames(type));
                    }
                }
                else
                {
                    res.Add(serviceType.Name);
                }
                return res;
            }
        }

        enum MethodParameterType
        {
            Route,
            Query,
            Body,
            Form,
            Special
        }
        private static string GeneratorMethodCode(INamedTypeSymbol serviceType, string instanceName, IMethodSymbol method)
        {
            var returnType = method.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var methodName = method.Name;
            var httpMethod = GetHttpMethod(method);
            var noBody = httpMethod == "HttpGet" || httpMethod == "HttpDelete";
            var comment = GetMethodComment(serviceType, method);
            var firstArgIsRoute = noBody && IsFirstRouteParameter(method);
            var route = firstArgIsRoute ? $"{methodName}/{{{method.Parameters[0].Name}}}" : methodName;
            var hasStreamParameter = method.Parameters.Any(p => p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "global::System.IO.Stream");
            var parameters = new List<string>();
            var args = new List<string>();
            var newClassLine = string.Empty;
            Func<IParameterSymbol, string> formatParam = (parameter) => $"{parameter.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {parameter.Name}";

            var allParameters = new List<(IParameterSymbol, MethodParameterType)>();

            for (var i = 0; i < method.Parameters.Length; i++)
            {
                if (i == 0 && firstArgIsRoute)
                {
                    allParameters.Add((method.Parameters[i], MethodParameterType.Route));
                }
                else if (IsSpecialType(method.Parameters[i].Type))
                {
                    allParameters.Add((method.Parameters[i], MethodParameterType.Special));
                }
                else if (hasStreamParameter)
                {
                    allParameters.Add((method.Parameters[i], MethodParameterType.Form));
                }
                else if (noBody)
                {
                    allParameters.Add((method.Parameters[i], MethodParameterType.Query));
                }
                else
                {
                    allParameters.Add((method.Parameters[i], MethodParameterType.Body));
                }
            }

            var moreThan1Body = allParameters.Count(p => p.Item2 == MethodParameterType.Body) > 1;

            if (!moreThan1Body || hasStreamParameter)
            {
                parameters.AddRange(allParameters.Select(p => p.Item2 switch
                {
                    MethodParameterType.Route => $"[FromRoute] {formatParam(p.Item1)}",
                    MethodParameterType.Query => $"[FromQuery] {formatParam(p.Item1)}",
                    MethodParameterType.Body => $"[FromBody] {formatParam(p.Item1)}",
                    MethodParameterType.Form => $"[FromForm] global::Microsoft.AspNetCore.Http.IFormFile {p.Item1.Name}",
                    _ => formatParam(p.Item1)
                }));
                args.AddRange(allParameters.Select(p => p.Item1.Name));
            }
            else
            {
                var bodyParameters = allParameters.Where(p => p.Item2 == MethodParameterType.Body).ToList();
                var newClassName = $"__{method.Name}_BodyArg";
                var newClassArgName = "arg";
                newClassLine = $"public record {newClassName}({string.Join(", ", bodyParameters.Select(t => t.Item1))});";
                parameters.AddRange(allParameters.Where(p => p.Item2 == MethodParameterType.Route).Select(p => $"[FromRoute] {formatParam(p.Item1)}"));
                parameters.AddRange(allParameters.Where(p => p.Item2 == MethodParameterType.Query).Select(p => $"[FromQuery] {formatParam(p.Item1)}"));
                parameters.Add($"[FromBody] {newClassName} {newClassArgName}");
                parameters.AddRange(allParameters.Where(p => p.Item2 == MethodParameterType.Special).Select(p => $"{formatParam(p.Item1)}"));
                args.AddRange(allParameters.Select(p => p.Item2 switch
                {
                    MethodParameterType.Body => $"{newClassArgName}.{p.Item1.Name}",
                    _ => p.Item1.Name
                }));
            }





            var paremeterLine = string.Join(", ", parameters);
            var argumentLine = string.Join(", ", args);
            return $@"{comment}
[Route(""{route}"")]
[{httpMethod}]
public {returnType} {methodName}({paremeterLine})
{{
    return this.{instanceName}.{methodName}({argumentLine});                    
}}

{newClassLine}
";
        }
        private static bool IsSpecialType(ITypeSymbol typeSymbol)
        {
            if (typeSymbol is INamedTypeSymbol namedType)
            {
                var typeFullName = namedType.ToDisplayFullString();
                return typeFullName == "global::System.Threading.CancellationToken"
                    || typeFullName == "global::Microsoft.AspNetCore.Http.HttpContext"
                    || typeFullName == "global::Microsoft.AspNetCore.Http.HttpRequest"
                    || typeFullName == "global::Microsoft.AspNetCore.Http.HttpResponse"
                    || typeFullName == "global::System.Security.Claims.ClaimsPrincipal";
            }
            return false;
        }
        private static string GetHttpMethod(IMethodSymbol method)
        {
            var hasReturnType = HasReturnType(method.ReturnType);
            var lowerMethodName = method.Name.ToLower();
            foreach (var rule in HttpMethodRules)
            {
                if (rule.Value.Any(prefix => lowerMethodName.StartsWith(prefix)))
                {
                    return rule.Key;
                }
            }
            // 根据返回值类型决定默认方法
            return HasReturnType(method.ReturnType) ? "HttpGet" : "HttpPost";
        }
        private static string GetMethodComment(INamedTypeSymbol serviceType, IMethodSymbol method)
        {
            //获取method上定义的原始xml注释
            var xmlComment = method.GetDocumentationCommentXml();
            if (string.IsNullOrWhiteSpace(xmlComment))
            {
                return string.Empty;
            }
            // 解析XML注释，提取<member>节点的子节点内容
            try
            {
                var xDoc = System.Xml.Linq.XDocument.Parse(xmlComment);
                var memberElement = xDoc.Element("member");
                if (memberElement == null || !memberElement.HasElements)
                {
                    return string.Empty;
                }

                // 创建StringBuilder来构建格式化的注释
                var formattedComment = new System.Text.StringBuilder();

                // 遍历<member>节点的所有子元素
                foreach (var child in memberElement.Elements())
                {
                    var elementName = child.Name.LocalName;
                    var isSingleLineElement = elementName == "param" || elementName == "returns";

                    if (isSingleLineElement)
                    {
                        // param 和 returns 标签在一行中显示
                        var textContent = !string.IsNullOrWhiteSpace(child.Value) ? child.Value.Trim() : "";
                        formattedComment.AppendLine($"///<{elementName}>{textContent}</{elementName}>");
                    }
                    else
                    {
                        // 其他标签（如 summary）保持多行格式
                        formattedComment.AppendLine($"/// <{elementName}>");

                        // 如果元素有文本内容，添加缩进的文本行
                        if (!string.IsNullOrWhiteSpace(child.Value))
                        {
                            foreach (var textLine in child.Value.Trim().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                formattedComment.AppendLine($"/// {textLine.Trim()}");
                            }
                        }

                        // 添加结束标签
                        formattedComment.AppendLine($"/// </{elementName}>");
                    }
                }

                return formattedComment.ToString().TrimEnd();
            }
            catch (System.Xml.XmlException)
            {
                // 如果所有方法都失败，返回空字符串
                return string.Empty;
            }
        }
        private static string GetClassComment(INamedTypeSymbol serviceType)
        {
            //获取method上定义的原始xml注释
            var xmlComment = serviceType.GetDocumentationCommentXml();
            if (string.IsNullOrWhiteSpace(xmlComment))
            {
                return string.Empty;
            }
            // 解析XML注释，提取<member>节点的子节点内容
            try
            {
                var xDoc = System.Xml.Linq.XDocument.Parse(xmlComment);
                var memberElement = xDoc.Element("member");
                if (memberElement == null || !memberElement.HasElements)
                {
                    return string.Empty;
                }

                // 创建StringBuilder来构建格式化的注释
                var formattedComment = new System.Text.StringBuilder();

                // 遍历<member>节点的所有子元素
                foreach (var child in memberElement.Elements())
                {
                    var elementName = child.Name.LocalName;
                    // 其他标签（如 summary）保持多行格式
                    formattedComment.AppendLine($"/// <{elementName}>");
                    // 如果元素有文本内容，添加缩进的文本行
                    if (!string.IsNullOrWhiteSpace(child.Value))
                    {
                        foreach (var textLine in child.Value.Trim().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            formattedComment.AppendLine($"/// {textLine.Trim()}");
                        }
                    }
                    // 添加结束标签
                    formattedComment.AppendLine($"/// </{elementName}>");
                }

                return formattedComment.ToString().TrimEnd();
            }
            catch (System.Xml.XmlException)
            {
                // 如果所有方法都失败，返回空字符串
                return string.Empty;
            }
        }
        private static bool IsFirstRouteParameter(IMethodSymbol method)
        {
            if (method.Parameters.Length > 1)
            {
                return RouteFieldNames.Contains(method.Parameters[0].Name);
            }
            return false;
        }
        private static bool HasReturnType(ITypeSymbol returnType)
        {
            if (returnType == null)
                return true;

            var returnTypeName = returnType.Name;

            // 处理Task类型
            if (returnTypeName == "Task" && returnType is INamedTypeSymbol taskType && taskType.IsGenericType)
            {
                // 对于Task<T>，获取T作为实际返回值类型
                var genericArgs = taskType.TypeArguments;
                if (genericArgs.Length > 0)
                {
                    return false;
                }
                // Task（无泛型参数）视为void
                return true;
            }
            // 处理ValueTask类型
            else if (returnTypeName == "ValueTask" && returnType is INamedTypeSymbol valueTaskType && valueTaskType.IsGenericType)
            {
                // 对于ValueTask<T>，获取T作为实际返回值类型
                var genericArgs = valueTaskType.TypeArguments;
                if (genericArgs.Length > 0)
                {
                    return false;
                }
                // ValueTask（无泛型参数）视为void
                return true;
            }

            // 非异步方法直接返回类型名称
            return false;
        }

        // 为了兼容性，保持ISourceGenerator接口的实现
        [Generator]
        public class ControllerGeneratorLegacy : ISourceGenerator
        {
            public void Initialize(GeneratorInitializationContext context)
            {
                // 对于ISourceGenerator，我们只需要注册，实际实现由IIncrementalGenerator完成
            }

            public void Execute(GeneratorExecutionContext context)
            {
                // 这里可以留空，因为实际工作由IIncrementalGenerator实现完成
            }
        }
    }
}

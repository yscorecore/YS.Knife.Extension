using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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
            var controllerName = GetControllerName(serviceTypeSymbol);

            // 确定服务名称：服务类型名称的驼峰式命名（首字母小写）
            var serviceName = GetServiceFieldName(serviceTypeSymbol);

            // 确定命名空间：与原始类相同
            var namespaceName = classDeclarationSyntax.Parent is NamespaceDeclarationSyntax namespaceDecl
                ? namespaceDecl.Name.ToString()
                : "";

            // 获取路由前缀
            var router = GetRouter() ?? "api/[controller]"; // 默认值

            var allowAnonymous = GetAllowAnonymous();

            CsharpCodeBuilder codeBuilder = new CsharpCodeBuilder();

            codeBuilder.AppendCodeLines($@"using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace {namespaceName}");
            codeBuilder.BeginSegment();
            codeBuilder.AppendCodeLines($@"[ApiController]
[Route(""{router}"")]
");
            if (allowAnonymous)
            {
                codeBuilder.AppendCodeLines("[AllowAnonymous]");
            }
            codeBuilder.AppendCodeLines($"public class {controllerName} : ControllerBase");
            codeBuilder.BeginSegment();
            //构造函数
            codeBuilder.AppendCodeLines($@"public {controllerName}({serviceType} {serviceName})
{{
    this.{serviceName} = {serviceName};
}}");
            codeBuilder.AppendCodeLines($@"private readonly {serviceType} {serviceName};");

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
                return serviceTypeSymbol.Name.TrimStart('I').ToCamelCase();
            }
            string GetControllerName(INamedTypeSymbol serviceType)
            {
                var name = serviceTypeSymbol.Name.TrimStart('I');
                if (name.EndsWith("Service"))
                {
                    name = name.Substring(0, name.Length - "Service".Length);
                }
                return $"{name}Controller";
            }
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

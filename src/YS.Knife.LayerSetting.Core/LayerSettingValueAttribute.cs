using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using YS.Knife.Function;
using YS.Knife.LogicRoles;

namespace YS.Knife
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public partial class LayerSettingValueAttribute : KnifeAttribute
    {
        public LayerSettingValueAttribute(params string[] roleProviders) : base(null)
        {
            RoleProviders = roleProviders;
        }
        public string Description { get; set; }
        public string Name { get; set; }
        public string Group { get; set; }

        public string[] RoleProviders { get; }

        public override void RegisterService(IServiceCollection services, IRegisterContext context, Type declareType)
        {
            services.AddScoped(typeof(ILayerSettingValue<>), typeof(LayerValueFactory<>));
        }

        [AutoConstructor]
        internal partial class LayerValueFactory<T> : ILayerSettingValue<T>
            where T : class, new()
        {
            private readonly ILayerService layerService;
            private readonly ILogicRoleService logicRoleService;
            [AutoConstructorIgnore]
            private T cacheValue = default;
            public async Task<T> Get()
            {
                if (cacheValue == default)
                {
                    var attribute = typeof(T).GetCustomAttribute<LayerSettingValueAttribute>();
                    var groupName = attribute?.Group ?? typeof(T).FullName;
                    var logicRoles = await logicRoleService.GetAllRoleCodes();
                    cacheValue = await layerService.GetLayerSettingObjectByRoleCodes<T>(groupName, logicRoles.FilterByProviders(attribute.RoleProviders));
                }
                return cacheValue;
            }



            public async Task<T> GetByRoles(IDictionary<string, string[]> roleMaps)
            {
                var attribute = typeof(T).GetCustomAttribute<LayerSettingValueAttribute>();
                var groupName = attribute.Group ?? typeof(T).FullName;
                var roleProviders = attribute.RoleProviders ?? Array.Empty<string>();
                var sortedRoleCodes = roleProviders.SelectMany(p =>
                {
                    if (roleMaps.TryGetValue(p, out var codes))
                    {
                        return codes.Select(t => $"{p}::{t}");
                    }
                    else
                    {
                        throw new Exception($"Can not find role value by provider '{p}'.");
                    }
                }).ToArray();
                return await layerService.GetLayerSettingObjectByRoleCodes<T>(groupName, sortedRoleCodes);
            }
            public async Task<TProp> GetPropertyValueByRoles<TProp>(IDictionary<string, string[]> roleMaps, string propertyName)
            {
                var attribute = typeof(T).GetCustomAttribute<LayerSettingValueAttribute>();
                var groupName = attribute.Group ?? typeof(T).FullName;
                var roleProviders = attribute.RoleProviders ?? Array.Empty<string>();
                var sortedRoleCodes = roleProviders.SelectMany(p =>
                {
                    if (roleMaps.TryGetValue(p, out var codes))
                    {
                        return codes.Select(t => $"{p}::{t}");
                    }
                    else
                    {
                        throw new Exception($"Can not find role value by provider '{p}'.");
                    }
                }).ToArray();

                var layerValue = await layerService.GetGroupValues(groupName, sortedRoleCodes, new string[] { propertyName });
                var firstValue = layerValue.FirstOrDefault()?.Value;
                return string.IsNullOrEmpty(firstValue) ? default : JsonSerializer.Deserialize<TProp>(firstValue, ILayerService.JsonOptions);

            }

            public Task<TProp> GetPropertyValueByRoles<TProp>(IDictionary<string, string[]> roleMaps, Expression<Func<T, TProp>> propertyFunc)
            {
                var propName = GetPropertyName(propertyFunc);
                propName = propName.WithStyle(NameStyle.CamelCase);
                return GetPropertyValueByRoles<TProp>(roleMaps, propName);
            }
            private static string GetPropertyName<TProp>(Expression<Func<T, TProp>> expression)
            {
                if (expression.Body is MemberExpression memberExpr)
                {
                    return memberExpr.Member.Name;
                }
                if (expression.Body is UnaryExpression unaryExpr && unaryExpr.Operand is MemberExpression memberExpr2)
                {
                    return memberExpr2.Member.Name;
                }
                throw new ArgumentException("Not a Member Expression.");
            }
        }

    }
}

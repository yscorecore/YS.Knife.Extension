using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using YS.Knife.Function;

namespace YS.Knife.LayerSetting
{
    public static class AssemblyExtensions
    {
        public static IEnumerable<SettingInfo> FindLayerSettings(this Assembly assembly)
        {
            return assembly.GetTypes()
                .Where(type => type.IsClass && !type.IsAbstract && type.GetCustomAttributes(typeof(LayerSettingValueAttribute), false).Any())
                .Select(CreateSetting);
        }

        private static SettingInfo CreateSetting(Type settingType)
        {
            var attribute = settingType.GetCustomAttribute<LayerSettingValueAttribute>();
            return attribute == null
                ? throw new ArgumentException($"Type {settingType.FullName} is not decorated with {nameof(LayerSettingValueAttribute)}.")
                : new SettingInfo
                {
                    Name = attribute.Name ?? settingType.Name,
                    Group = attribute.Group ?? settingType.FullName,
                    Description = attribute.Description,
                    RoleProviders = attribute.RoleProviders ?? Array.Empty<string>(),
                    Properties = settingType.GetProperties().Where(p => p.CanWrite && p.CanWrite)
                    .Select(p => CreateSettingPropertyInfo(p)).ToList()
                };
        }
        private static (bool, string) GetTypeCode(Type type)
        {
            if (type.IsArray)
            {
                return (true, GetTypeCode(type.GetElementType()));
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
        private static SettingPropertyInfo CreateSettingPropertyInfo(PropertyInfo propertyInfo)
        {
            var displayAttribute = propertyInfo.GetCustomAttribute<DisplayAttribute>();
            var displayNameAttribute = propertyInfo.GetCustomAttribute<DisplayNameAttribute>();
            var descriptionAttribute = propertyInfo.GetCustomAttribute<DescriptionAttribute>();
            var (isArray, typeCode) = GetTypeCode(propertyInfo.PropertyType);
            var editor = propertyInfo.GetCustomAttribute<Metadata.EditorSourceAttribute>(true)?.ToString();
            return new SettingPropertyInfo
            {
                Key = propertyInfo.Name.WithStyle(NameStyle.CamelCase),
                Name = displayAttribute?.Name ?? displayNameAttribute?.DisplayName,
                Description = displayAttribute?.Description ?? descriptionAttribute?.Description,
                Order = displayAttribute?.Order ?? 0,
                DataSource = editor,
                IsArray = isArray,
                Type = typeCode,
            };
        }


    }
}

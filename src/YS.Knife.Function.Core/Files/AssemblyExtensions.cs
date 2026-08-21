using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using YS.Knife.Function.Files;
using YS.Knife.Operations;

namespace YS.Knife.Function.Files
{
    public static class AssemblyExtensions
    {
        public static AppInfo FindAppInfo(this Assembly entryAssembly, Assembly[] assemblies, Func<Type, bool> isModuleFunc, Func<MethodInfo, bool> isActionFunc)
        {
            var appInfo = GetAppInfo(entryAssembly);
            foreach (var assembly in new Assembly[] { entryAssembly }.Concat(assemblies).Distinct())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (isModuleFunc(type))
                    {
                        var module = GetModuleInfo(type);
                        appInfo.Modules!.Add(module);
                        var actions = GetModuleActionInfos(type, isActionFunc);
                        if (module.Actions == null) module.Actions = new List<ActionInfo>();
                        module.Actions.AddRange(actions);
                    }
                }
            }
            return appInfo;

        }
        private static AppInfo GetAppInfo(Assembly assembly)
        {
            var appatt = assembly.GetCustomAttribute<AppAttribute>();
            if (appatt == null) throw new Exception($"The assembly '{assembly.FullName}' hasn't define '{typeof(AppAttribute).FullName}'.");
            return new AppInfo
            {
                AppId = appatt.Id,
                AppDesc = appatt.Description,
                AppName = appatt.Description!,
                AppConfig = new Dictionary<string, object>
                {
                    { "roleProviders", appatt.RoleProviders },
                    { "assembly", assembly.FullName ?? string.Empty}
                },
                Modules = new List<ModuleInfo>()
            };
        }
        private static ModuleInfo GetModuleInfo(Type type)
        {
            var attr = type.GetCustomAttribute<ModuleAttribute>();
            return attr != null ? new ModuleInfo
            {
                Code = attr.Id,
                Desc = attr.Description,
                Name = attr.Description,
                Config = new Dictionary<string, object> { { "type", type.FullName ?? string.Empty } },
            } : new ModuleInfo
            {
                Code = type.Name,
                Name = type.Name,
                Desc = type.FullName ?? string.Empty,
                Config = new Dictionary<string, object> { { "type", type.FullName ?? string.Empty } }
            };
        }
        private static List<ActionInfo> GetModuleActionInfos(Type type, Func<MethodInfo, bool> isActionFunc)
        {
            return type.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => !p.IsSpecialName)
                .Where(p => isActionFunc(p))
                .Select(p => GetMethodActionInfo(p))
                .ToList();
        }
        private static ActionInfo GetMethodActionInfo(MethodInfo method)
        {
            var op = method.GetOperation();
            return new ActionInfo { Code = op.Id, Name = op.Description, Desc = op.Description, Config = new Dictionary<string, object> { { "method", method.Name } } };
        }
    }
}

using System.Reflection;
using System.Text.Json;

namespace YS.Knife.Operations
{
    [SingletonPattern]
    public partial class FunctionFile
    {
        static JsonSerializerOptions JsonSerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };
        internal record AppInfo(string appId, string appName, string appDesc, string[] roleProviders);
        internal record ModuleInfo(string code, string desc, Type type);
        internal record ActionInfo(string code, string desc, MethodInfo method);
        public async Task<Stream> DumpJson(Assembly entryAssembly, Assembly[] assemblies, Func<Type, bool> isModuleFunc)
        {
            var appInfo = GetAppInfo(entryAssembly);
            var modules = new List<(ModuleInfo, List<ActionInfo>)>();
            foreach (var assembly in new Assembly[] { entryAssembly }.Concat(assemblies).Distinct())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (isModuleFunc(type))
                    {
                        modules.Add((GetModuleInfo(type), GetModuleActionInfos(type)));
                    }
                }
            }
            var obj = new
            {
                appInfo.appId,
                appInfo.appName,
                appInfo.appDesc,
                appConfig = new
                {
                    appInfo.roleProviders,
                    assembly = entryAssembly.FullName
                },
                modules = modules.Select(p => new
                {
                    p.Item1.code,
                    p.Item1.desc,
                    name = p.Item1.desc,
                    config = new
                    {
                        type = p.Item1.type.FullName,
                    },
                    actions = p.Item2.Select(t => new
                    {
                        t.code,
                        t.desc,
                        name = t.desc,
                        config = new
                        {
                            method = t.method.Name
                        }
                    }).ToArray()
                }).ToArray()
            };
            var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync(ms, obj, JsonSerializerOptions);
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }
        private AppInfo GetAppInfo(Assembly assembly)
        {
            var appatt = Assembly.GetEntryAssembly()?.GetCustomAttribute<AppAttribute>();
            if (appatt == null) throw new Exception($"Entry assembly '{assembly.FullName}' hasn't define '{typeof(AppAttribute).FullName}'.");
            return new AppInfo(appatt.Id, assembly.FullName, appatt.Description, appatt.RoleProviders);
        }
        private ModuleInfo GetModuleInfo(Type type)
        {
            if (type == null) return null;
            var attr = type.GetCustomAttribute<ModuleAttribute>();
            return attr != null ? new ModuleInfo(attr.Id, attr.Description, type) : new ModuleInfo(type.FullName, type.FullName, type);
        }
        private List<ActionInfo> GetModuleActionInfos(Type type)
        {
            return type.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => !p.IsSpecialName)
                .Select(p => GetMethodActionInfo(p))
                .ToList();
        }
        private static ActionInfo GetMethodActionInfo(MethodInfo method)
        {
            var op = method.GetOperation();
            return new ActionInfo(op.Id, op.Description, method);
        }
    }
}

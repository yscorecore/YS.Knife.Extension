using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;

namespace YS.Knife.DataItem.Api.AspnetCore.Internal
{

    partial class DynamicType
    {
        private DynamicType()
        {
            moduleBuilder1 = new Lazy<ModuleBuilder>(GetModuleBuilder);
        }
        public static DynamicType Instance = new DynamicType();
        Lazy<ModuleBuilder> moduleBuilder1;
        ConcurrentDictionary<string, Type> caches = new ConcurrentDictionary<string, Type>();
        public Type Create(string name)
        {
            return caches.GetOrAdd(name, (t) =>
            {
                return moduleBuilder1.Value.DefineType(t, TypeAttributes.Public).CreateType();
            });
        }
        private ModuleBuilder GetModuleBuilder()
        {
            var assemblyName = new AssemblyName("DataItem_DynamicAssembly");
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            return assemblyBuilder.DefineDynamicModule("DataItem_DynamicModule");
        }
    }
}

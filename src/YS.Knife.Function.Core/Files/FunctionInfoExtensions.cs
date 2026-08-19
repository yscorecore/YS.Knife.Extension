using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YS.Knife.Function;
using YS.Knife.Query;

namespace YS.Knife.Function.Files
{
    public static class FunctionInfoExtensions
    {
        public static List<FunctionInfo> ToFunctionModel(this AppInfo appInfo)
        {
            if (string.IsNullOrEmpty(appInfo.AppId))
            {
                throw new Exception("AppId is Empty.");
            }
            var functions = ExpandToFunctionInfo(appInfo);
            CheckFunctions(functions);
            return functions;
        }
        static List<FunctionInfo> ExpandToFunctionInfo(AppInfo appInfo)
        {
            List<FunctionInfo> result = new List<FunctionInfo>();
            result.Add(new FunctionInfo
            {
                Code = appInfo.AppId,
                Description = appInfo.AppDesc,
                ParentCode = null,
                Name = appInfo.AppName,
                Sequence = 0,
                Type = "app",
                Config = appInfo.AppConfig,
            });
            foreach (var (module, index) in (appInfo.Modules ?? Enumerable.Empty<ModuleInfo>()).Select((p, i) => (p, i)))
            {
                ExpandModule(appInfo.AppId, module, index * 100);
            }

            return result;

            void ExpandModule(string parentCode, ModuleInfo moduleInfo, int sequence)
            {
                if (moduleInfo == null)
                {
                    return;
                }
                result.Add(new FunctionInfo
                {
                    Code = moduleInfo.Code,
                    Description = moduleInfo.Desc,
                    ParentCode = parentCode,
                    Name = moduleInfo.Name,
                    Sequence = sequence,
                    Type = "module",
                    Config = moduleInfo.Config,
                });
                foreach (var (module, index) in (moduleInfo.Modules ?? Enumerable.Empty<ModuleInfo>()).Select((p, i) => (p, i)))
                {
                    ExpandModule(moduleInfo.Code, module, index);
                }
                foreach (var (action, index) in (moduleInfo.Actions ?? Enumerable.Empty<ActionInfo>()).Select((p, i) => (p, i)))
                {
                    ExpandAction(moduleInfo.Code, action, index);
                }
            }
            void ExpandAction(string baseModuleCode, ActionInfo actionInfo, int sequence)
            {
                if (actionInfo == null)
                {
                    return;
                }
                result.Add(new FunctionInfo
                {
                    Code = $"{baseModuleCode}::{actionInfo.Code}",
                    Description = actionInfo.Desc,
                    ParentCode = baseModuleCode,
                    Name = actionInfo.Name,
                    Sequence = sequence,
                    Config = actionInfo.Config,
                    Type = "action",
                });
            }
        }


        static void CheckFunctions(List<FunctionInfo> functions)
        {
            var item = functions.ToLookup(p => p.Code).Where(p => p.Count() > 1).FirstOrDefault();
            if (item != null)
            {
                throw new Exception($"Duplicate function code '{item.Key}'");
            }
        }
    }
}

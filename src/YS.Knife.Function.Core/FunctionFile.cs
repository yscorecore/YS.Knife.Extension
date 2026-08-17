using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using YS.Knife.Function.Core;

namespace YS.Knife.Function
{
    [SingletonPattern]
    public partial class FunctionFile
    {
        static JsonSerializerOptions JsonSerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            IgnoreReadOnlyProperties = false,
            PropertyNameCaseInsensitive = false,
            ReadCommentHandling = JsonCommentHandling.Skip,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };
        public async Task<List<FunctionInfo>> LoadFromFile(StreamBody file, CancellationToken cancellationToken)
        {
            var appInfo = await LoadAppInfoFromFile(file);
            if (appInfo == null)
            {
                throw new Exception("不是有效的function文件");
            }
            var functions = ExpandToFunctionInfo(appInfo);
            CheckFunctions(functions);
            return functions;
        }



        async ValueTask<AppInfo?> LoadAppInfoFromFile(StreamBody file)
        {
            return await JsonSerializer.DeserializeAsync<AppInfo>(file.Stream, JsonSerializerOptions);
        }
        List<FunctionInfo> ExpandToFunctionInfo(AppInfo appInfo)
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


        void CheckFunctions(List<FunctionInfo> functions)
        {
            var item = functions.ToLookup(p => p.Code).Where(p => p.Count() > 1).FirstOrDefault();
            if (item != null)
            {
                throw new Exception($"duplicate function code '{item.Key}'");
            }
        }

        record AppInfo
        {
            public string AppId { get; set; } = null!;
            public string AppName { get; set; } = null!;
            public string? AppDesc { get; set; } = null!;
            public Dictionary<string, object> AppConfig { get; set; } = null!;
            public List<ModuleInfo>? Modules { get; set; } = null!;


        }
        record ModuleInfo
        {
            public string Code { get; set; } = null!;
            public string Name { get; set; } = null!;
            public string? Desc { get; set; } = null!;
            public Dictionary<string, object>? Config { get; set; } = null!;
            public List<ModuleInfo>? Modules { get; set; } = null!;
            public List<ActionInfo>? Actions { get; set; } = null!;
        }
        record ActionInfo
        {
            public string Code { get; set; } = null!;
            public string Name { get; set; } = null!;
            public string? Desc { get; set; } = null!;
            public Dictionary<string, object>? Config { get; set; } = null!;
        }
    }
}

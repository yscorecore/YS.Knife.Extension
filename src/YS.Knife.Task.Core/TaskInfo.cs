using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace YS.Knife.Task
{
    public record class TaskInfo
    {

        public static JsonSerializerOptions JsonSerializerOptions { get; set; } = new JsonSerializerOptions()
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        public string Name { get; set; } = null!;
        public string? Version { get; set; }
        public object Argument { get; set; } = null!;


        public static TaskInfo<TArg> FromObject<TArg>(TArg arg, string name, string? version)
            where TArg : new()
        {
            return new TaskInfo<TArg>
            {
                Argument = arg,
                Name = name,
                Version = version
            };
        }
        public static TaskInfo FromText(string argumentJsonText, string name, string? version)
        {
            return new TaskInfo
            {
                Name = name,
                Version = version,
                Argument = argumentJsonText,
            };
        }
        public static TaskInfo<TArg> FromTask<TTask, TArg>(TArg arg)
            where TTask : ITask<TArg>
            where TArg : class, new()
        {
            var attr = TaskAttribute.GetFromType<TTask>();
            return new TaskInfo<TArg>
            {
                Name = attr.Name,
                Version = attr.Version,
                Argument = arg
            };

        }
        public string GetArgumentString()
        {
            if (Argument is string str)
            {
                return str;
            }
            else
            {
                return this.Argument.ToJsonText(JsonSerializerOptions);
            }
        }
    }

    public record class TaskInfo<T> : TaskInfo
    {
        public new T Argument
        {
            get { return (T)base.Argument; }
            set { base.Argument = value!; }
        }

    }
}

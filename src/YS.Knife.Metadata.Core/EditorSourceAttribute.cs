using System.Text.Json;

namespace YS.Knife.Metadata
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public abstract class EditorSourceAttribute : Attribute
    {
        public static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
        public EditorSourceAttribute(SourceType type, object value, object config = null)
        {
            this.SourceType = type;
            this.Value = value;
            this.Config = config;
        }
        public string Expression { get => ToString(); }
        public SourceType SourceType { get; }
        public object Value { get; }
        public object Config { get; }

        public override string ToString()
        {
            if (Config == null)
            {
                return $"{this.SourceType.ToString().ToLowerInvariant()}({JsonSerializer.Serialize(Value, JsonOptions)})";
            }
            else
            {
                return $"{this.SourceType.ToString().ToLowerInvariant()}({JsonSerializer.Serialize(Value, JsonOptions)}, {JsonSerializer.Serialize(Config, JsonOptions)})";
            }

        }
    }

    public enum SourceType
    {
        DataSource = 0,
        Enum = 1,
        Const = 2,
        Dic = 3
    }

    public record DataSourceConfig(string RefName, bool IsArray, bool SingleProperty, string[] Properties);

    public record EnumSourceConfig(bool Flag);

    public record DicSourceConfig(bool Flag);

    public class EditorDataSourceAttribute : EditorSourceAttribute
    {
        public EditorDataSourceAttribute(string dataSourceName, string refName = null, bool isArray = false, bool singleProperty = true, params string[] properties) :
            base(SourceType.DataSource, dataSourceName, new DataSourceConfig(refName, isArray, singleProperty, properties))
        {
        }
    }
    public class EditorEnumSourceAttribute : EditorSourceAttribute
    {
        public EditorEnumSourceAttribute(string enumName, bool flag = false) :
            base(SourceType.Enum, enumName, new EnumSourceConfig(flag))
        {
        }
    }
    public class EditorConstantSourceAttribute : EditorSourceAttribute
    {
        public EditorConstantSourceAttribute(params object[] values)
            : base(SourceType.Const, values)
        {
        }
    }
    public class EditorNamedConstantSourceAttribute : EditorSourceAttribute
    {
        public EditorNamedConstantSourceAttribute(string[] names, string[] values, bool flag)
            : base(SourceType.Dic,
                  names.Zip(values).ToDictionary(p => p.First, p => p.Second), new DicSourceConfig(flag))
        {
        }
    }
    /// <summary>
    /// 声明列的绑定数据源，与编辑器数据源（DataSource）并行存在，二者可同时标注在同一属性上。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class BindToSourceAttribute : Attribute
    {
        public BindToSourceAttribute(string refName = null, bool isArray = false, bool singleProperty = true, params string[] properties)
        {
            this.Config = new DataSourceConfig(refName, isArray, singleProperty, properties);
        }
        public DataSourceConfig Config { get; }
    }
}

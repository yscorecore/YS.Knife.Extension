using System.Text.Json;

namespace YS.Knife.Metadata
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public abstract class EditorSourceAttribute : Attribute
    {
        public EditorSourceAttribute(SourceType type, object value)
        {
            this.SourceType = type;
            this.Value = value;
        }
        public string Expression { get => ToString(); }
        public SourceType SourceType { get; }
        public object Value { get; }

        public override string ToString()
        {
            return $"{this.SourceType.ToString()}({JsonSerializer.Serialize(Value)})";
        }
    }

    public enum SourceType
    {
        DataSource = 0,
        Enum = 1,
        Constant = 2,
        NamedConstant = 3
    }
    public class EditorDataSourceAttribute : EditorSourceAttribute
    {
        public EditorDataSourceAttribute(string dataSourceName) :
            base(SourceType.DataSource, dataSourceName)
        {
        }
    }
    public class EditorEnumSourceAttribute : EditorSourceAttribute
    {
        public EditorEnumSourceAttribute(string enumName) :
            base(SourceType.Enum, enumName)
        {
        }
    }
    public class EditorConstantSourceAttribute : EditorSourceAttribute
    {
        public EditorConstantSourceAttribute(params object[] values)
            : base(SourceType.Constant, values)
        {
        }
    }
    public class EditorNamedConstantSourceAttribute : EditorSourceAttribute
    {
        public EditorNamedConstantSourceAttribute(string[] names, object[] values)
            : base(SourceType.NamedConstant,
                  names.Zip(values).ToDictionary(p => p.First, p => p.Second))
        {
        }
    }
}

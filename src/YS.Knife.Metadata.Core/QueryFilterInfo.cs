namespace YS.Knife.Metadata
{
    public record QueryFilterInfo
    {
        public object DefaultValue { get; set; }
        public string Operator { get; set; } = Operators.Equal;
    }

    public sealed class Operators
    {
        public const string Equal = "==";
        public const string NotEqual = "!=";
        public const string GreaterThan = ">";
        public const string LessThanOrEqual = "<=";
        public const string LessThan = "<";
        public const string GreaterThanOrEqual = ">=";
        public const string Between = "between";
        public const string NotBetween = "not_between";
        public const string In = "in";
        public const string NotIn = "not_in";
        public const string StartsWith = "startswith";
        public const string NotStartsWith = "not_startswith";
        public const string EndsWith = "endswith";
        public const string NotEndsWith = "not_endswith";
        public const string Contains = "contains";
        public const string NotContains = "not_contains";
    }
}

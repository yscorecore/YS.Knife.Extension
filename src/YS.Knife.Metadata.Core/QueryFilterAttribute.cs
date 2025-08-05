namespace YS.Knife.Metadata
{
    [AttributeUsage(AttributeTargets.Property)]
    public class QueryFilterAttribute : Attribute
    {
        public QueryFilterAttribute(string @operator = Operators.Equal)
        {
            this.Operator = @operator;
        }
        public object DefaultValue { get; set; }
        public string Operator { get; }
    }
}

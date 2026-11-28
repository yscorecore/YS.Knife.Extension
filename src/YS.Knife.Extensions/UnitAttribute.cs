namespace YS.Knife
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class UnitAttribute : TagAttribute
    {
        public const string TagKey = "unit";
        public UnitAttribute(string unit) : base(TagKey, unit) { }
        public string Unit => (string)Value;
    }
}

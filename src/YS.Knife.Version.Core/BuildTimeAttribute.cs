namespace YS.Knife.Version
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = true)]
    public class BuildTimeAttribute : Attribute
    {
        public DateTime BuildTime { get; }
        public BuildTimeAttribute(string buildTime)
        {
            BuildTime = DateTime.Parse(buildTime);
        }
    }

}

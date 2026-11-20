namespace YS.Knife.Metadata
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class MetadataInterceptorAttribute : Attribute
    {
        public MetadataInterceptorAttribute(string interceptorName)
        {
            _ = interceptorName ?? throw new ArgumentNullException(nameof(interceptorName));
            InterceptorName = interceptorName;
        }

        public string InterceptorName { get; }
    }
}

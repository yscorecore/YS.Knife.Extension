namespace Microsoft.EntityFrameworkCore
{
    public abstract class ProviderAttribute : Attribute
    {
        public string Provider { get; set; }
    }
}

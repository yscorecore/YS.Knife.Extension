namespace Microsoft.EntityFrameworkCore
{
    public abstract class ProviderAttribute : Attribute, IProviderAttribute
    {
        public string Provider { get; set; }
    }
}

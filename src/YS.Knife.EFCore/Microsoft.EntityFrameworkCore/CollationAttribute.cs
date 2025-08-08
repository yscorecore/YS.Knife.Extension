using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore
{
    public class CollationAttribute : ProviderAttribute, IModelPropertyAttribute, IModelAttribute
    {
        public CollationAttribute(string collation)
        {
            Collation = collation;
        }
        public string Collation { get; set; }
        public void Apply(IMutableProperty property)
        {
            property.SetCollation(Collation);
        }

        public void Apply(IMutableModel model)
        {
            model.SetCollation(Collation);
        }
    }
}

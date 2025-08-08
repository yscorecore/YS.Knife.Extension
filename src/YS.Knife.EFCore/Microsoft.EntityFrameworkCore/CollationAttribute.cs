using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore
{
    public class CollationAttribute : ProviderAttribute, IModelPropertyAttribute, IModelAttribute
    {
        public CollationAttribute(string collation)
        {
            Collation = collation;
        }
        public string Collation { get; set; }
        public void Apply(ModelBuilder modelBuilder)
        {
            modelBuilder.Model.SetCollation(Collation);
        }

        public void Apply(PropertyBuilder propertyBuilder)
        {
            propertyBuilder.Metadata.SetCollation(Collation);
        }
    }
}

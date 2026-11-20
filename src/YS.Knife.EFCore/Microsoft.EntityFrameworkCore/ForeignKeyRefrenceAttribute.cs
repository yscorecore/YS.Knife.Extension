using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore
{
    [System.AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class ForeignKeyRefrenceAttribute : ProviderAttribute, IModelTypeAttribute
    {
        public ForeignKeyRefrenceAttribute(string foreignKey, string? relatedMember, string? collectionMember, string principalKey)
            : this(new string[] { foreignKey }, relatedMember, collectionMember, new string[] { principalKey })
        {

        }
        public ForeignKeyRefrenceAttribute(string[] foreignKeys, string? relatedMember, string? collectionMember, string[] principalKeys)
        {
            ForeignKey = foreignKeys;
            RelatedMember = relatedMember;
            CollectionMember = collectionMember;
            PrincipalKey = principalKeys;
        }

        public string[] ForeignKey { get; }
        public string? RelatedMember { get; }
        public string? CollectionMember { get; }
        public string[] PrincipalKey { get; }
        public bool IsRequired { get; set; } = true;
        public void Apply(EntityTypeBuilder typeBuilder)
        {
            typeBuilder.HasOne(this.RelatedMember)
                .WithMany(this.CollectionMember)
                .HasForeignKey(this.ForeignKey)
                .HasPrincipalKey(this.PrincipalKey)
                .IsRequired(true);
        }
    }

}

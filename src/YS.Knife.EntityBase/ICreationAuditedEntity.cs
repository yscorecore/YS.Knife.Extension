using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YS.Knife.Entity
{




    public interface ICreationAuditedEntity
    {
        DateTimeOffset CreationTime { get; set; }
        string Creator { get; set; }  // 用 ? 消除 CS8616 警告
    }

    // 删除审计（统一用 Deleter / Deletion）
    public interface IDeletionAuditedEntity : ISoftDeleteEntity
    {
        DateTimeOffset? DeletionTime { get; set; }
        string DeleterId { get; set; }
    }
    public interface IModificationAuditedEntity
    {
        DateTimeOffset? LastModificationTime { get; set; }
        string LastModifier { get; set; }
    }
    public interface IFullAuditedEntity : ICreationAuditedEntity, IModificationAuditedEntity, IDeletionAuditedEntity
    {

    }
    public class CreationAuditedEntity<TKey> : IEntity<TKey>, ICreationAuditedEntity
          where TKey : notnull
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual TKey Id { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public virtual DateTimeOffset CreationTime { get; set; }
        [StringLength(64)]
        public virtual string Creator { get; set; }
    }
    public class FullAuditedEntity<TKey> : CreationAuditedEntity<TKey>, IFullAuditedEntity
        where TKey : notnull
    {
        public DateTimeOffset? LastModificationTime { get; set; }
        public string LastModifier { get; set; }
        public DateTimeOffset? DeletionTime { get; set; }
        public string DeleterId { get; set; }
        public bool IsDeleted { get; set; }
    }
}

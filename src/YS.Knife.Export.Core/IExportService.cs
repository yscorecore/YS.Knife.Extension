using System.ComponentModel.DataAnnotations;

namespace YS.Knife.Export
{
    public interface IExportService
    {
        Task<ExportToken> BeginExport(EntityMetadata[] metadatas);

        Task Export(Guid token, EntityData data);

        Task<bool> CancelExport(Guid token);

        Task<Stream> EndExport(Guid token);
    }
    public record ExportToken
    {
        public Guid Token { get; set; }
        public int ExpiredIn { get; set; }
    }
    public record EntityColumnMetadata
    {
        [Required]
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public int? Width { get; set; }
        public DataType DataType { get; set; }
    }
    public record EntityMetadata
    {
        [Required]
        public string Name { get; set; }
        public string DisplayName { get; set; }
        [Required]
        public List<EntityColumnMetadata> Columns { get; set; }
    }
    public enum DataType
    {
        String,
        Number,
        Boolean,
        DateTime,
    }
    public record EntityData
    {
        public string Name { get; set; }
        public List<Dictionary<string, object>> Datas { get; set; }
    }
}

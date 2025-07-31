using System.ComponentModel.DataAnnotations;

namespace YS.Knife.Import.Abstractions
{
    public interface IImportService
    {
        Task<ImportToken> BeginImport(Stream data, string fileExt);
        Task<Dictionary<string, List<Dictionary<string, object>>>> ReadData(Guid token, EntityMetadata[] entityMetadata);
        Task<bool> EndImport(Guid token);
        Task<Dictionary<string, List<ColumnInfo>>> ReadColumns(Guid token);
    }

    public record ColumnInfo
    {
        public string ColumnName { get; set; }
    }
    public record ImportToken
    {
        public Guid Token { get; set; }
        public int ExpiredIn { get; set; }
    }

    public record EntityMetadata
    {
        [Required]
        public string Name { get; set; }
        public int? Position { get; set; }
        public string[] Alias { get; set; }
        [Required]
        public List<EntityColumnMetadata> Columns { get; set; }
    }
    public record EntityColumnMetadata
    {
        [Required]
        public string Name { get; set; }
        public int? Position { get; set; }
        public string[] Alias { get; set; }
        public DataType DataType { get; set; }
        public bool Optional { get; set; }
    }
    public enum DataType
    {
        String,
        Boolean,
        DateTime,
        Number,
    }

}

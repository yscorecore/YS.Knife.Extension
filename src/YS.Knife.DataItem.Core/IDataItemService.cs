namespace YS.Knife.DataItem
{
    public interface IDataItemService
    {
        Task<object> GetItem(string name, object[] args, CancellationToken cancellationToken);

        Task<List<DataItemDesc>> GetAllDataItems();

        Task<DataItemEntry> GetEntry(string name);
    }
    public record DataItemDesc
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}

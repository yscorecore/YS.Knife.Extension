namespace YS.Knife.Service
{
    public interface ISaveApi<TSaveDto>
    {
        Task Save(TSaveDto[] data, CollectionSaveMode saveMode = CollectionSaveMode.Merge, CancellationToken token = default);
    }
}
